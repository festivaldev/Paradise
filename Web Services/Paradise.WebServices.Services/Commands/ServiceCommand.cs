using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Paradise.WebServices {
	internal class ServiceCommand : ParadiseCommand {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ServiceCommand));

		public static new string Command => "service";
		public static new string[] Aliases => new string[] { "svc" };

		public override string Description => "Allows starting and stopping of services.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  status\t\tDisplay the current status of all known services.",
			"  start <name>\t\tAttempts to start a service named \"name\".",
			"  stop <name>\t\tStops a service named \"name\".",
			"  restart <name>\tRestarts a service named \"name\".",
			"  start-all\t\tAttempts to start all known services.",
			"  stop-all\t\tStops all known services.",
			"  restart-all\t\tRestarts all known services."
		};

		public ServiceCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "status":
					if (arguments.Length == 2 && !string.IsNullOrWhiteSpace(arguments[1])) {
						if (ParadiseService.Instance.Services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(arguments[1].ToLower())).Count > 1) {
							WriteLine($"{Command}: Could not find service matching {arguments[2]}");
						} else {
							if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
								var service = ParadiseService.Instance.Services[serviceName];
								WriteLine($"{service.ServiceName}[{service.ServiceVersion}]: {service.State}");
							} else {
								WriteLine($"{Command}: Unknown service {arguments[1]}");
							}
						}
					} else {
						foreach (KeyValuePair<string, BaseWebService> entry in ParadiseService.Instance.Services) {
							WriteLine($"{entry.Value.ServiceName}[{entry.Value.ServiceVersion}]: {entry.Value.State}");
						}
					}
					break;
				case "start-all":
					WriteLine("Starting all services...");
					foreach (var service in ParadiseService.Instance.Services.Values) {
						service.StartService();
					}
					break;
				case "stop-all":
					WriteLine("Stopping all services...");
					foreach (var service in ParadiseService.Instance.Services.Values) {
						service.StopService();
					}
					break;
				case "restart-all":
					WriteLine("Restarting all services...");
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
						WriteLine($"{Command}: Could not find service matching {arguments[1]}");
					} else {
						if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
							var service = ParadiseService.Instance.Services[serviceName];
							if (!service.StartService()) {
								WriteLine($"Failed to start service {serviceName}.");
							}
						} else {
							WriteLine($"{Command}: Unknown service {arguments[1]}");
						}
					}

					break;
				}
				case "stop": {
					if (ParadiseService.Instance.Services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(arguments[1].ToLower())).Count > 1) {
						WriteLine($"{Command}: Could not find service matching {arguments[1]}");
					} else {
						if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
							var service = ParadiseService.Instance.Services[serviceName];
							if (!service.StopService()) {
								WriteLine($"Failed to stop service {serviceName}.");
							}
						} else {
							WriteLine($"{Command}: Unknown service {arguments[1]}");
						}
					}

					break;
				}
				case "restart": {
					if (ParadiseService.Instance.Services.Keys.ToList().FindAll(_ => _.ToLower().StartsWith(arguments[1].ToLower())).Count > 1) {
						WriteLine($"{Command}: Could not find service matching {arguments[1]}");
					} else {
						if (ParadiseService.Instance.Services.Keys.ToList().Find(_ => _.ToLower().StartsWith(arguments[1].ToLower())) is string serviceName) {
							var service = ParadiseService.Instance.Services[serviceName];
							WriteLine($"Restarting service {serviceName}...");
							service.StopService();

							Thread.Sleep(500);

							if (!service.StartService()) {
								WriteLine($"Failed to restart service {serviceName}.");
							}
						} else {
							WriteLine($"{Command}: Unknown service {arguments[1]}");
						}
					}

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
	}
}
