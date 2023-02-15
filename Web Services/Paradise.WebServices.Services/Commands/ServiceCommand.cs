using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Paradise.WebServices {
	internal class ServiceCommand : IParadiseCommand {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ServiceCommand));

		public string Command => "service";
		public string[] Alias => new string[] { "svc" };

		public string Description => "Allows starting and stopping of services.";
		public string HelpString => $"{Command}\t\t{Description}";

		public void Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "status":
					if (arguments.Length == 2 && !string.IsNullOrWhiteSpace(arguments[1])) {
						if (ParadiseService.Instance.Services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(arguments[1].ToLower())).Count > 1) {
							CommandHandler.WriteLine($"{Command}: Could not find service matching {arguments[2]}");
						} else {
							if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
								var service = ParadiseService.Instance.Services[serviceName];
								CommandHandler.WriteLine($"{service.ServiceName}[{service.ServiceVersion}]: {service.State}");
							} else {
								CommandHandler.WriteLine($"{Command}: Unknown service {arguments[1]}");
							}
						}
					} else {
						foreach (KeyValuePair<string, BaseWebService> entry in ParadiseService.Instance.Services) {
							CommandHandler.WriteLine($"{entry.Value.ServiceName}[{entry.Value.ServiceVersion}]: {entry.Value.State}");
						}
					}
					break;
				case "start-all":
					CommandHandler.WriteLine("Starting all services...");
					foreach (var service in ParadiseService.Instance.Services.Values) {
						service.StartService();
					}
					break;
				case "stop-all":
					CommandHandler.WriteLine("Stopping all services...");
					foreach (var service in ParadiseService.Instance.Services.Values) {
						service.StopService();
					}
					break;
				case "restart-all":
					CommandHandler.WriteLine("Restarting all services...");
					foreach (var service in ParadiseService.Instance.Services.Values) {
						service.StopService();
					}

					Thread.Sleep(500);

					foreach (var service in ParadiseService.Instance.Services.Values) {
						service.StartService();
					}
					break;
				case "start": {
					if (ParadiseService.Instance.Services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(arguments[1].ToLower())).Count > 1) {
						CommandHandler.WriteLine($"{Command}: Could not find service matching {arguments[1]}");
					} else {
						if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
							var service = ParadiseService.Instance.Services[serviceName];
							if (!service.StartService()) {
								CommandHandler.WriteLine($"Failed to start service {serviceName}.");
							}
						} else {
							CommandHandler.WriteLine($"{Command}: Unknown service {arguments[1]}");
						}
					}

					break;
				}
				case "stop": {
					if (ParadiseService.Instance.Services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(arguments[1].ToLower())).Count > 1) {
						CommandHandler.WriteLine($"{Command}: Could not find service matching {arguments[1]}");
					} else {
						if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
							var service = ParadiseService.Instance.Services[serviceName];
							if (!service.StopService()) {
								CommandHandler.WriteLine($"Failed to stop service {serviceName}.");
							}
						} else {
							CommandHandler.WriteLine($"{Command}: Unknown service {arguments[1]}");
						}
					}

					break;
				}
				case "restart": {
					if (ParadiseService.Instance.Services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(arguments[1].ToLower())).Count > 1) {
						CommandHandler.WriteLine($"{Command}: Could not find service matching {arguments[1]}");
					} else {
						if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
							var service = ParadiseService.Instance.Services[serviceName];
							CommandHandler.WriteLine($"Restarting service {serviceName}...");
							service.StopService();

							Thread.Sleep(500);

							if (!service.StartService()) {
								CommandHandler.WriteLine($"Failed to restart service {serviceName}.");
							}
						} else {
							CommandHandler.WriteLine($"{Command}: Unknown service {arguments[1]}");
						}
					}

					break;
				}
				default:
					CommandHandler.WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}

		public void PrintUsageText() {
			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine("  status\t\tDisplay the current status of all known services.");
			CommandHandler.WriteLine("  start <name>\t\tAttempts to start a service named \"name\".");
			CommandHandler.WriteLine("  stop <name>\t\tStops a service named \"name\".");
			CommandHandler.WriteLine("  restart <name>\tRestarts a service named \"name\".");
			CommandHandler.WriteLine("  start-all\t\tAttempts to start all known services.");
			CommandHandler.WriteLine("  stop-all\t\tStops all known services.");
			CommandHandler.WriteLine("  restart-all\t\tRestarts all known services.");
		}
	}
}
