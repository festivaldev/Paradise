using log4net;
using log4net.Config;
using System;
using System.Configuration.Assemblies;
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

		[STAThread]
		static void Main(string[] args) {
			foreach (var arg in args) {
				switch (arg) {
					case "--install":
						try {
							ManagedInstallerClass.InstallHelper(new string[] { "/InstallStateDir=", "/LogFile=", "/LogToConsole=true", Assembly.GetExecutingAssembly().Location });
						} catch (Exception e) {
							MessageBox.Show($"Failed to install Paradise Web Services: {e.Message}\n{e.StackTrace}", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Error);
							Environment.Exit(1);
						}

						MessageBox.Show("Paradise Web Services have been successfully installed!", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Information);
						Environment.Exit(0);

						break;
					case "--uninstall":
						try {
							ManagedInstallerClass.InstallHelper(new string[] { "/u", "/LogFile=", "/LogToConsole=true", Assembly.GetExecutingAssembly().Location });
						} catch (Exception e) {
							MessageBox.Show($"Failed to remove Paradise Web Services: {e.Message}\n{e.StackTrace}", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Error);
							Environment.Exit(0);
						}

						MessageBox.Show("Paradise Web Services have been successfully removed!", "Paradise Web Services", MessageBoxButtons.OK, MessageBoxIcon.Information);
						Environment.Exit(0);

						break;
					case "-c":
					case "--console":
						RunMode = RunMode.Console;
						break;
					case "--svc":
					case "--service":
						RunMode = RunMode.Service;
						break;
					case "--tray":
					case "--gui":   // Deprecated: use --tray instead
						RunMode = RunMode.WinForms;
						break;
					default: break;
				}
			}

			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolvePluginDependency);
			switch (RunMode) {
				case RunMode.Console:
					ConfigureLogging();

					ConsoleHelper.CreateConsole();
					SetConsoleCtrlHandler(consoleEventHandler, true);

					ServiceInstance = new ParadiseService();
					ServiceInstance.Start();

					ConsoleHelper.PrintConsoleHeaderSubtitle();

					using (var host = new ServiceHost(typeof(ParadiseService))) {
						host.AddServiceEndpoint(typeof(IParadiseServiceHost), new NetNamedPipeBinding(), "net.pipe://localhost/NewParadise.WebServices");
						host.Open();

						while (RunApp) {
							var cmd = ConsolePrompter.Prompt("> ");
							var cmdArgs = cmd.Split(' ').ToList();

							switch (cmdArgs[0].ToLower()) {
								case "clear":
									Console.Clear();

									ConsoleHelper.PrintConsoleHeader();
									ConsoleHelper.PrintConsoleHeaderSubtitle();
									break;
								case "help":
									ConsoleHelper.PrintConsoleHelp();
									break;
								case "q":
								case "quit":
									ConsoleEventCallback(CtrlType.CTRL_CLOSE_EVENT);
									Environment.Exit(0);
									break;
								default:
									CommandHandler.HandleCommand(cmdArgs[0], cmdArgs.Skip(1).Take(cmdArgs.Count - 1).ToArray());
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
						host.AddServiceEndpoint(typeof(IParadiseServiceHost), new NetNamedPipeBinding(), "net.pipe://localhost/NewParadise.WebServices");
						host.Open();

						ServiceBase.Run(new ParadiseService());

						host.Close();
					}

					break;
				case RunMode.WinForms:
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					new ParadiseControlForm();

					Application.Run();

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

		static bool ConsoleEventCallback(CtrlType eventType) {
			if (eventType == CtrlType.CTRL_CLOSE_EVENT) {
				RunApp = false;
				Console.WriteLine("Bye.");

				ServiceInstance?.Teardown();

				Thread.Sleep(300);
			}

			return true;
		}

		static Assembly ResolvePluginDependency(object sender, ResolveEventArgs args) {
			string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string assemblyFile = $"{new AssemblyName(args.Name).Name}.dll";

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
