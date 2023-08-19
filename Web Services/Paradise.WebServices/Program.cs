using CommandLine;
using log4net;
using log4net.Config;
using System;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Paradise.WebServices {
	internal class Program {
		private static readonly ILog Log = LogManager.GetLogger(nameof(Program));

		private static RunMode RunMode = RunMode.Console;
		private static bool RunApp = true;

		private static ParadiseService ServiceInstance;

		internal delegate bool EventHandler(CtrlType sig);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

		internal enum CtrlType {
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		internal static EventHandler consoleEventHandler = new EventHandler(ConsoleEventCallback);

		public static CLIOptions CLIOptions { get; private set; }

		[STAThread]
		static void Main(string[] args) {
			Parser.Default.ParseArguments<CLIOptions>(args)
				.WithParsed<CLIOptions>(o => {
					CLIOptions = o;

					if (o.InstallService) {
						try {
							ManagedInstallerClass.InstallHelper(new string[] { "/InstallStateDir=", "/LogFile=", "/LogToConsole=true", Assembly.GetExecutingAssembly().Location });
						} catch (Exception e) {
							if (!o.Silent) {
								MessageBox.Show($"Failed to install Paradise Web Services: {e.Message}\n{e.StackTrace}", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}

							Environment.Exit(1);
						}

						if (!o.Silent) {
							MessageBox.Show("Paradise Web Services have been successfully installed!", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}

						Console.ReadLine();
						Environment.Exit(0);
					} else if (o.UninstallService) {
						try {
							ManagedInstallerClass.InstallHelper(new string[] { "/u", "/LogFile=", "/LogToConsole=true", Assembly.GetExecutingAssembly().Location });
						} catch (Exception e) {
							if (!o.Silent) {
								MessageBox.Show($"Failed to remove Paradise Web Services: {e.Message}\n{e.StackTrace}", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}

							Environment.Exit(1);
						}

						if (!o.Silent) {
							MessageBox.Show("Paradise Web Services have been successfully removed!", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}

						Environment.Exit(0);
					}

					if (o.ConsoleMode) {
						RunMode = RunMode.Console;
					} else if (o.ServiceMode) {
						RunMode = RunMode.Service;
					} else if (o.TrayMode) {
						RunMode = RunMode.WinForms;
					} else if (o.GUIMode) {
						MessageBox.Show("The \"--gui\" launch parameter is deprecated, please use \"--tray\" instead.", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Information);
						Environment.Exit(0);
					}
				});

			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolvePluginDependency);

			switch (RunMode) {
				case RunMode.Console:
					ConfigureLogging();

					ConsoleHelper.CreateConsole();
					SetConsoleCtrlHandler(consoleEventHandler, true);
					ConsoleHelper.PrintConsoleHeader();

					ServiceInstance = new ParadiseService();
					ServiceInstance.Start();

					ConsoleHelper.PrintConsoleHeaderSubtitle();

					using (var host = new ServiceHost(ServiceInstance)) {
						// Add service endpoint to control Paradise via the tray app
						host.AddServiceEndpoint(typeof(IParadiseServiceHost), new NetNamedPipeBinding(), $"net.pipe://localhost/{Program.CLIOptions.PipeName ?? "NewParadise.WebServices"}");
						host.Open();

						while (RunApp) {
							var cmd = ConsolePrompter.Prompt("> ");
							var cmdArgs = cmd.Split(' ').ToList();

							switch (cmdArgs.FirstOrDefault().ToLower()) {
								case "clear":
									Console.Clear();

									ConsoleHelper.PrintConsoleHeader();
									ConsoleHelper.PrintConsoleHeaderSubtitle();
									break;
								case "q":
								case "quit":
									ConsoleEventCallback(CtrlType.CTRL_CLOSE_EVENT);
									Environment.Exit(0);
									break;
								default:
									CommandHandler.HandleCommand(cmdArgs.First(), cmdArgs.Skip(1).Take(cmdArgs.Count - 1).ToArray(), default,
										(string output, bool inline) => {
											if (!inline) {
												Console.WriteLine(output);
											} else {
												Console.Write(output);
											}
										},
										(ParadiseCommand invoker, bool success, string error) => {
											if (success && string.IsNullOrWhiteSpace(error)) {
												//Console.WriteLine(invoker.Output);
											} else {
												Console.WriteLine(error);
											}
										});

									break;
							}
						}

						host.Close();
					}

					ConsoleHelper.DestroyConsole();

					break;
				case RunMode.Service:
					ConfigureLogging();

					using (var host = new ServiceHost(typeof(ParadiseService))) {
						host.AddServiceEndpoint(typeof(IParadiseServiceHost), new NetNamedPipeBinding(), $"net.pipe://localhost/{Program.CLIOptions.PipeName ?? "NewParadise.WebServices"}");
						host.Open();

						ServiceBase.Run(new ParadiseService());

						host.Close();
					}

					break;
				case RunMode.WinForms:
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					Application.Run(new ParadiseControlForm());

					break;
				default: break;
			}
		}

		private static void ConfigureLogging() {
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Paradise.WebServices.log4net.config")) {
				using (StreamReader reader = new StreamReader(stream)) {
					var logConfig = new XmlDocument();
					logConfig.LoadXml(reader.ReadToEnd());

					XmlConfigurator.Configure(logConfig.DocumentElement);
				}
			}
		}

		private static bool ConsoleEventCallback(CtrlType eventType) {
			if (eventType == CtrlType.CTRL_CLOSE_EVENT) {
				RunApp = false;
				ServiceInstance?.Teardown();

				Console.WriteLine("Bye.");

				Thread.Sleep((int)TimeSpan.FromSeconds(0.5).TotalMilliseconds);
			}

			return true;
		}

		private static Assembly ResolvePluginDependency(object sender, ResolveEventArgs args) {
			string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string assemblyFile = $"{new AssemblyName(args.Name).Name}.dll";

			foreach (var pluginDir in Directory.GetDirectories(Path.Combine(folderPath, "Plugins"))) {
				if (!pluginDir.EndsWith(".plugin")) continue;

				if (File.Exists(Path.Combine(folderPath, "Plugins", pluginDir, assemblyFile))) {
					return Assembly.LoadFrom(Path.Combine(folderPath, "Plugins", pluginDir, assemblyFile));
				}
			}

			if (File.Exists(Path.Combine(folderPath, "Plugins", assemblyFile))) {
				return Assembly.LoadFrom(Path.Combine(folderPath, "Plugins", assemblyFile));
			}

			if (File.Exists(Path.Combine(folderPath, assemblyFile))) {
				return Assembly.LoadFrom(Path.Combine(folderPath, assemblyFile));
			}

			return null;
		}
	}
}
