using log4net;
using log4net.Config;
using System;
using System.Configuration.Install;
using System.Diagnostics;
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

		private static RunMode RunMode = RunMode.WinForms;


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

		private static bool RunApp = true;

		[STAThread]
		public static void Main(string[] args) {
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
						RunMode = RunMode.Console;
						break;
					case "--svc":
						RunMode = RunMode.Service;
						break;
					case "--gui":
						RunMode = RunMode.WinForms;
						break;
					default: break;
				}
			}

			switch (RunMode) {
				case RunMode.Console:
					ConfigureLogging();

					ConsoleHelper.CreateConsole();

					SetConsoleCtrlHandler(consoleEventHandler, true);

					var serviceInstance = new ParadiseService();
					serviceInstance.Start();

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

		static void ConfigureLogging() {
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

				DatabaseManager.DisposeDatabase();

				Console.WriteLine("Bye.");
				Thread.Sleep(300);
			}

			return true;
		}
	}
}
