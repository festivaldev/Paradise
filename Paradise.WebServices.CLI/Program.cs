using Paradise.WebServices.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.WebServices.CLI {
	class Program {
		#region ANSI color code support
		private const int STD_OUTPUT_HANDLE = -11;
		private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
		private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

		[DllImport("kernel32.dll")]
		private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();
		#endregion

		private delegate bool ConsoleEventDelegate(int eventType);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

		private static BasicHttpBinding httpBinding;
		static Dictionary<string, WebServiceBase> services;
		private static ParadiseSettings webServiceSettings;

		static void Main(string[] args) {
			var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
			GetConsoleMode(iStdOut, out uint outConsoleMode);
			outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
			SetConsoleMode(iStdOut, outConsoleMode);

			SetConsoleCtrlHandler(new ConsoleEventDelegate(ConsoleEventCallback), true);

			XmlSerializer ser = new XmlSerializer(typeof(ParadiseSettings));

			using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "ParadiseSettings.xml")))) {
				try {
					webServiceSettings = (ParadiseSettings)ser.Deserialize(reader);
				} catch (Exception e) {
					Log.Error(e.Message);
				}
			}

			PrintHeader();

			DatabaseManager.OpenDatabase();

			httpBinding = new BasicHttpBinding();
			httpBinding.Security.Mode = BasicHttpSecurityMode.None;

			services = new Dictionary<string, WebServiceBase> {
				["Application"] = new ApplicationWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				["Authentication"] = new AuthenticationWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				["Clan"] = new ClanWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				["Moderation"] = new ModerationWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				["PrivateMessage"] = new PrivateMessageWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				["Relationship"] = new RelationshipWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				["Shop"] = new ShopWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				["User"] = new UserWebService(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),

				//["Application_Legacy"] = new ApplicationWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				//["Authentication_Legacy"] = new AuthenticationWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				//["Clan_Legacy"] = new ClanWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				//["Moderation_Legacy"] = new ModerationWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				//["PrivateMessage_Legacy"] = new PrivateMessageWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				//["Relationship_Legacy"] = new RelationshipWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				//["Shop_Legacy"] = new ShopWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
				//["User_Legacy"] = new UserWebService_Legacy(httpBinding, webServiceSettings.WebServiceBaseUrl, webServiceSettings.WebServicePrefix, webServiceSettings.WebServiceSuffix),
			};

			Console.WriteLine();
			PrintHeaderSubtitle();

			while (true) {
				Console.Write("> ");
				var input = Console.ReadLine();
				var cmdArgs = input.Split(' ');

				switch (cmdArgs[0].ToLower()) {
					case "":
						break;
					case "clear":
						Console.Clear();

						PrintHeader();
						PrintHeaderSubtitle();
						break;
					case "database":
						if (cmdArgs.Length < 2) {
							Console.WriteLine("database: Controls the LiteDB database instance." + Environment.NewLine);
							Console.WriteLine("close\t\t\tSaves the database and closes the instance.");
							Console.WriteLine("open\t\t\tOpens a new database instance.");
							Console.WriteLine("reload\t\t\tReloads the current database instance");
						} else {
							switch (cmdArgs[1].ToLower()) {
								case "close":
									DatabaseManager.DisposeDatabase();
									break;
								case "open":
									DatabaseManager.OpenDatabase();
									break;
								case "reload":
									DatabaseManager.ReloadDatabase();
									break;
								default:
									Console.WriteLine($"database: unknown command {cmdArgs[1]}");
									break;
							}
						}
						break;
					case "help":
						PrintHelp();
						break;
					case "service":
						if (cmdArgs.Length < 2) {
							Console.WriteLine("service: Allows starting and stopping of services." + Environment.NewLine);
							Console.WriteLine("status\t\t\tDisplay the current status of all known services.");
							Console.WriteLine("start [name]\t\tAttempts to start a service named \"name\".");
							Console.WriteLine("stop [name]\t\tStops a service named \"name\".");
							Console.WriteLine("restart [name]\t\tRestarts a service named \"name\".");
							Console.WriteLine("start-all\t\tAttempts to start all known services.");
							Console.WriteLine("stop-all\t\tStops all known services.");
							Console.WriteLine("restart-all\t\tRestarts all known services.");
						} else {
							switch (cmdArgs[1].ToLower()) {
								case "status":
									foreach (KeyValuePair<string, WebServiceBase> entry in services) {
										Console.WriteLine($"{entry.Key}({entry.Value.ServiceVersion}): {entry.Value.State}");
									}
									break;
								case "start-all":
									foreach (WebServiceBase service in services.Values) {
										service.StartService();
									}
									break;
								case "stop-all":
									foreach (WebServiceBase service in services.Values) {
										service.StopService();
									}
									break;
								case "restart-all":
									foreach (WebServiceBase service in services.Values) {
										service.StopService();
									}

									Thread.Sleep(500);

									foreach (WebServiceBase service in services.Values) {
										service.StartService();
									}
									break;
								case "start": {
										if (services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(cmdArgs[2].ToLower())).Count > 1) {
											Console.WriteLine($"service: No unique service found matching {cmdArgs[2]}");
										} else {
											if (services.Keys.ToList().Find(_ => _.ToLower().StartsWith(cmdArgs[2].ToLower())) is string serviceName) {
												var service = services[serviceName];
												if (!service.StartService()) {
													Log.Error($"Failed to start service {serviceName}.");
												}
											} else {
												Console.WriteLine($"service: Unknown service {cmdArgs[2]}");
											}
										}

										break;
									}
								case "stop": {
										if (services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(cmdArgs[2].ToLower())).Count > 1) {
											Console.WriteLine($"service: No unique service found matching {cmdArgs[2]}");
										} else {
											if (services.Keys.ToList().Find(_ => _.ToLower().StartsWith(cmdArgs[2].ToLower())) is string serviceName) {
												var service = services[serviceName];
												if (!service.StopService()) {
													Log.Error($"Failed to stop service {serviceName}.");
												}
											} else {
												Console.WriteLine($"service: Unknown service {cmdArgs[2]}");
											}
										}

										break;
									}
								case "restart": {
										if (services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(cmdArgs[2].ToLower())).Count > 1) {
											Console.WriteLine($"service: No unique service found matching {cmdArgs[2]}");
										} else {
											if (services.Keys.ToList().Find(_ => _.ToLower().StartsWith(cmdArgs[2].ToLower())) is string serviceName) {
												var service = services[serviceName];
												Log.Info($"Restarting service {serviceName}...");
												service.StopService();
												Thread.Sleep(500);
												if (!service.StartService()) {
													Log.Error($"Failed to restart service {serviceName}.");
												}
											} else {
												Console.WriteLine($"service: Unknown service {cmdArgs[2]}");
											}
										}

										break;
									}
								default:
									Console.WriteLine($"service: unknown command {cmdArgs[1]}");
									break;
							}
						}
						break;
					case "q":
					case "quit":
						ConsoleEventCallback(2);

						Environment.Exit(0);
						break;
					default:
						Console.WriteLine($"{cmdArgs[0]}: Unkown command.");
						break;
				}
			}
		}

		static void PrintHeader() {
			Console.WriteLine($"Project Paradise - Web Services [Version {typeof(Program).Assembly.GetName().Version}]");
			Console.WriteLine("(c) 2017, 2022 Team FESTIVAL. All rights reserved." + Environment.NewLine);
		}

		static void PrintHeaderSubtitle() {
			Console.WriteLine("Type \"help\" to see available commands.");
		}

		static void PrintHelp() {
			Console.WriteLine("Available commands:" + Environment.NewLine);
			Console.WriteLine("clear\t\tClears the console, duh.");
			Console.WriteLine("help\t\tPrints this help text.");
			Console.WriteLine("service\t\tAllows starting and stopping of services.");
			Console.WriteLine("quit\t\tQuits the application.");
		}


		static bool ConsoleEventCallback(int eventType) {
			if (eventType == 2) {
				DatabaseManager.DisposeDatabase();

				Console.WriteLine("Bye.");
				Thread.Sleep(300);
			}

			return false;
		}
	}
}
