using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.WebServices {
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
	public partial class ParadiseService : ServiceBase, IParadiseServiceHost, IServiceCallback {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseService));

		public static ParadiseService Instance { get; private set; }

		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		private static BasicHttpBinding HttpBinding;
		public List<ParadiseServicePlugin> Plugins = new List<ParadiseServicePlugin>();
		public Dictionary<string, BaseWebService> Services { get; private set; } = new Dictionary<string, BaseWebService>();

		public static ParadiseServerSettings WebServiceSettings { get; private set; }
		private HttpServer HttpServer;

		public IParadiseServiceClient ClientCallback {
			get {
				return OperationContext.Current?.GetCallbackChannel<IParadiseServiceClient>();
			}
		}

		public ParadiseService() {
			InitializeComponent();
		}

		public void Start() {
			OnStart(new string[] { });
		}

		protected override void OnStart(string[] args) {
			base.OnStart(args);

			Instance = this;

			XmlSerializer serializer = new XmlSerializer(typeof(ParadiseServerSettings));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(CurrentDirectory, "Paradise.Settings.WebServices.xml")))) {
					WebServiceSettings = (ParadiseServerSettings)serializer.Deserialize(reader);
				}
			} catch (Exception e) {
				Log.Error("There was an error parsing the settings file.", e);
				//ClientCallback?.OnError("There was an error parsing the settings file.", e.Message);
			}

			CommandHandler.CommandOutput += (sender, callbackArgs) => {
				if (sender is ParadiseCommand) {
					ClientCallback?.OnConsoleCommandCallback(callbackArgs.Text);
				}
			};

			//DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			//DatabaseManager.DatabaseClosed += OnDatabaseClosed;
			//DatabaseManager.OpenDatabase();

			var uriBuilder = new UriBuilder {
				Scheme = WebServiceSettings.EnableSSL ? "https" : "http",
				Host = string.IsNullOrEmpty(WebServiceSettings.FileServerHostName) ? "*" : WebServiceSettings.FileServerHostName,
				Port = WebServiceSettings.FileServerPort
			};

			HttpServer = new HttpServer(new string[] { uriBuilder.ToString() });
			HttpServer.Use(new ParadiseRouter(Path.Combine(CurrentDirectory, WebServiceSettings.FileServerRoot), WebServiceSettings));

			try {
				HttpServer.Start();

				ClientCallback?.OnHttpServerStarted();
				Log.Info($"HTTP server listening on port {WebServiceSettings.FileServerPort} (using SSL: {(WebServiceSettings.EnableSSL ? "yes" : "no")}).");
			} catch (Exception e) {
				Log.Error(e);
				ClientCallback?.OnHttpServerError(e);
			}

			HttpBinding = new BasicHttpBinding();

			if (string.IsNullOrWhiteSpace(WebServiceSettings.WebServiceHostName)) {
				HttpBinding.HostNameComparisonMode = HostNameComparisonMode.WeakWildcard;
			}

			if (WebServiceSettings.EnableSSL) {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
				HttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
			} else {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.None;
			}

			if (Directory.Exists(Path.Combine(CurrentDirectory, "plugins"))) {
				foreach (string file in Directory.GetFiles(Path.Combine(CurrentDirectory, "Plugins"))) {
					if (file.EndsWith(".dll")) {
						try {
							Assembly assembly = Assembly.LoadFrom(file);

							foreach (Type type in assembly.GetTypes()) {
								if (typeof(ParadiseServicePlugin).IsAssignableFrom(type)) {
									Log.Info($"Loading plugin {Path.GetFileName(file)}...");

									LoadPlugin((ParadiseServicePlugin)Activator.CreateInstance(type));
									break;
								}
							}
						} catch (Exception e) {
							Log.Error($"Error while loading service plugin {Path.GetFileName(file)}:", e);
						}
					}
				}
			}

			foreach (var plugin in Plugins) {
				plugin.OnStart();
			}

			foreach (var service in Services.Values) {
				service.StartService();
			}
		}

		protected override void OnShutdown() {
			base.OnShutdown();

			Teardown();
		}

		protected override void OnStop() {
			base.OnStop();

			Teardown();
		}


		public void Teardown() {
			base.OnStop();

			HttpServer.Instance?.Stop();

			foreach (var service in Services.Values) {
				service.StopService();
			}

			foreach (var plugin in Plugins) {
				plugin.OnStop();
			}
		}

		private void LoadPlugin(ParadiseServicePlugin plugin) {
			Plugins.Add(plugin);

			if (plugin.Commands != null) {
				CommandHandler.Commands.AddRange(plugin.Commands);
			}

			var services = plugin.LoadServices(HttpBinding, WebServiceSettings, this);
			if (services != null) {
				foreach (var service in services) {
					Services.Add(service.Key, service.Value);
				}
			}

			plugin.OnLoad();
		}

		#region Service Callbacks
		public void OnServiceStarted(object sender, ServiceEventArgs args) {
			ClientCallback?.OnServiceStarted(args);
		}

		public void OnServiceStopped(object sender, ServiceEventArgs args) {
			ClientCallback?.OnServiceStopped(args);
		}

		public void OnServiceError(object sender, ServiceEventArgs args) {
			ClientCallback?.OnServiceError(args);
		}
		#endregion

		#region IParadiseServiceHost
		public bool StartService(string serviceName, string serviceVersion) {
			if (Instance.Services.ContainsKey(serviceName)) {
				return Instance.Services[serviceName].StartService();
			} else if (Instance.Services.ContainsKey(serviceName.Replace("WebService", ""))) {
				return Instance.Services[serviceName.Replace("WebService", "")].StartService();
			}

			return false;
		}

		public bool StopService(string serviceName, string serviceVersion) {
			if (Instance.Services.ContainsKey(serviceName)) {
				return Instance.Services[serviceName].StopService();
			} else if (Instance.Services.ContainsKey(serviceName.Replace("WebService", ""))) {
				return Instance.Services[serviceName.Replace("WebService", "")].StopService();
			}

			return false;
		}

		public bool RestartService(string serviceName, string serviceVersion) {
			if (Instance.Services.ContainsKey(serviceName)) {
				if (!Instance.Services[serviceName].StopService()) return false;

				return Instance.Services[serviceName].StartService();
			}

			return false;
		}

		public void StartAllServices() {
			foreach (var service in Instance.Services.Values) {
				service.StartService();
			}
		}

		public void StopAllServices() {
			foreach (var service in Instance.Services.Values) {
				service.StopService();
			}
		}

		public void RestartAllServices() {
			foreach (var service in Instance.Services.Values) {
				service.StopService();

				if (!service.StartService()) {
					ClientCallback?.OnError("Service Restart Failed", $"Service {service.ServiceName} [{service.ServiceVersion}] failed to restart. Please check the log file for further information.");
				}
			}
		}

		public bool IsAnyServiceRunning() {
			return Instance.Services.Values.Any(_ => _.IsRunning);
		}

		public bool IsDatabaseOpen() {
			throw new NotImplementedException();
		}

		public void OpenDatabase() {
			throw new NotImplementedException();
		}

		public void DisposeDatabase() {
			throw new NotImplementedException();
		}

		public bool IsHttpServerRunning() {
			return HttpServer.Instance?.IsRunning ?? false;
		}

		public void StartHttpServer() {
			if (!HttpServer.Instance?.IsRunning ?? false) {
				try {
					HttpServer.Instance.Start();

					ClientCallback?.OnHttpServerStarted();
					Log.Info($"HTTP server listening on port {WebServiceSettings.FileServerPort} (using SSL: {(WebServiceSettings.EnableSSL ? "yes" : "no")}).");
				} catch (Exception e) {
					Log.Error(e);
					ClientCallback?.OnHttpServerError(e);
				}
			}
		}

		public void StopHttpServer() {
			if (HttpServer.Instance?.IsRunning ?? false) {
				try {
					HttpServer.Instance.Stop();

					ClientCallback?.OnHttpServerStopped();
				} catch (Exception e) {
					Log.Error(e);
					ClientCallback?.OnHttpServerError(e);
				}
			}
		}

		public void SendConsoleCommand(string command, string[] arguments) {
			//Console.WriteLine(string.Join(" ", command, string.Join(" ", arguments)));
			CommandHandler.HandleCommand(command, arguments);
		}
		#endregion

		public ParadiseServiceStatus UpdateClientInfo() {
			var services = new List<Dictionary<string, object>>();

			foreach (var service in Instance.Services) {
				services.Add(new Dictionary<string, object> {
					["ServiceName"] = service.Value.ServiceName,
					["ServiceVersion"] = service.Value.ServiceVersion,
					["IsRunning"] = service.Value.IsRunning
				});
			}

			return new ParadiseServiceStatus {
				Services = services,
				DatabaseOpened = false,
				FileServerRunning = this.IsHttpServerRunning()
			};
		}
	}
}
