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
using static Paradise.TcpSocket;

namespace Paradise.WebServices {
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
	public partial class ParadiseService : ServiceBase, IParadiseServiceHost, IServiceCallback {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseService));

		public static ParadiseService Instance { get; private set; }
		public static ParadiseServerSettings WebServiceSettings { get; private set; }

		public static string WorkingDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		public List<ParadiseServicePlugin> Plugins = new List<ParadiseServicePlugin>();
		public Dictionary<string, BaseWebService> Services { get; private set; } = new Dictionary<string, BaseWebService>();

		private HttpServer HttpServer;
		private BasicHttpBinding HttpBinding;

		public TcpSocket.SocketServer SocketServer { get; private set; }
		public Dictionary<Guid, TcpSocket.SocketConnection> CommandInvokers = new Dictionary<Guid, TcpSocket.SocketConnection>();

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

			CommandHandler.Commands.AddRange(new List<Type> {
				typeof(DatabaseCommand),
				typeof(HelpCommand),
				typeof(ServerCommand),
				typeof(ServiceCommand)
			});

			#region Config
			XmlSerializer serializer = new XmlSerializer(typeof(ParadiseServerSettings));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(WorkingDirectory, "Paradise.Settings.WebServices.xml")), new XmlReaderSettings { IgnoreComments = true })) {
					WebServiceSettings = (ParadiseServerSettings)serializer.Deserialize(reader);
				}
			} catch (Exception e) {
				Log.Error("There was an error parsing the settings file.", e);
				ClientCallback?.OnError("There was an error parsing the settings file.", e.Message);
			}
			#endregion

			#region HTTP Server Setup
			var uriBuilder = new UriBuilder {
				Scheme = WebServiceSettings.EnableSSL ? "https" : "http",
				Host = string.IsNullOrEmpty(WebServiceSettings.Hostname) ? "*" : WebServiceSettings.Hostname,
				Port = WebServiceSettings.FileServerPort
			};

			HttpServer = new HttpServer(new string[] { uriBuilder.ToString() });
			HttpServer.Use(new ParadiseRouter(Path.Combine(WorkingDirectory, WebServiceSettings.FileServerRoot), WebServiceSettings));

			try {
				HttpServer.Start();

				ClientCallback?.OnHttpServerStarted();
				Log.Info($"HTTP server listening on {uriBuilder.Host}:{uriBuilder.Port} (using SSL: {(WebServiceSettings.EnableSSL ? "yes" : "no")}).");
			} catch (Exception e) {
				Log.Error(e);
				ClientCallback?.OnHttpServerError(e);
			}
			#endregion

			#region HTTP Binding Setup
			HttpBinding = new BasicHttpBinding();

			if (string.IsNullOrWhiteSpace(WebServiceSettings.Hostname)) {
				HttpBinding.HostNameComparisonMode = HostNameComparisonMode.WeakWildcard;
			}

			if (WebServiceSettings.EnableSSL) {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
				HttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
			} else {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.None;
			}
			#endregion

			#region Socket Server
			SocketServer = new TcpSocket.SocketServer(WebServiceSettings.TCPCommPort);
			SocketServer.Listen();

			SocketServer.ConnectionRejected += (sender, e) => {
				Log.Warn($"[Socket] Rejecting {e.Socket.Type}Server({e.Socket.Identifier}) from {e.Socket.Socket.RemoteEndPoint}. Reason: {e.Reason}");
			};

			SocketServer.ClientConnected += (sender, e) => {
				Log.Info($"[Socket] {e.Socket.Type}Server({e.Socket.Identifier}) connected from {e.Socket.Socket.RemoteEndPoint}.");
			};

			SocketServer.ClientDisconnected += (sender, e) => {
				Log.Info($"[Socket] {e.Socket.Type}Server({e.Socket.Identifier}) disconnected. Reason: {e.Reason}");
			};

			SocketServer.DataReceived += (sender, e) => {
				switch (e.Type) {
					case PacketType.Command:
						var cmd = (SocketCommand)e.Data;

						switch (cmd.Command) {
							case "link":
								// Prevent the command from being executed as it will be handled by the Discord plugin
								break;
							default:
								CommandHandler.HandleUserCommand(cmd.Command, cmd.Arguments, cmd.Invoker, default, null,
									(ParadiseCommand invoker, bool success, string error) => {
										if (success && string.IsNullOrWhiteSpace(error)) {
											e.Socket.SendSync(PacketType.CommandOutput, invoker.Output, true, e.Payload.ConversationId);
										} else {
											e.Socket.SendSync(PacketType.CommandOutput, error, true, e.Payload.ConversationId);
										}
									});
								break;
						}

						break;
					case PacketType.Monitoring:
						switch (e.Payload.ServerType) {
							case ServerType.Comm:
								ParadiseServerMonitoring.SetCommMonitoringData((Dictionary<string, object>)e.Data);
								break;
							case ServerType.Game:
								ParadiseServerMonitoring.SetGameMonitoringData(e.Socket.Identifier.ToString(), (Dictionary<string, object>)e.Data);
								break;
						}
						break;
				}
			};
			#endregion

			#region Plugins
			if (Directory.Exists(Path.Combine(WorkingDirectory, "plugins"))) {
				foreach (var pluginDir in Directory.GetDirectories(Path.Combine(WorkingDirectory, "Plugins"))) {
					if (!pluginDir.EndsWith(".plugin")) continue;
					if (WebServiceSettings.PluginBlacklist != null && WebServiceSettings.PluginBlacklist.Contains(Path.GetFileName(pluginDir)))
						continue;

					foreach (var file in Directory.GetFiles(pluginDir)) {
						if (!file.EndsWith(".dll")) continue;

						try {
							Assembly assembly = Assembly.LoadFrom(file);

							if (WebServiceSettings.PluginBlacklist != null && WebServiceSettings.PluginBlacklist.Contains(assembly.GetName().Name))
								continue;

							foreach (Type type in assembly.GetTypes()) {
								if (typeof(ParadiseServicePlugin).IsAssignableFrom(type)) {
									Log.Info($"Loading plugin {Path.GetFileName(file)}...");

									LoadPlugin((ParadiseServicePlugin)Activator.CreateInstance(type));
									break;
								}
							}
						} catch (ReflectionTypeLoadException e) {
							Log.Error($"Error while loading required assemblies for service plugin {Path.GetFileName(file)}:", e);

							foreach (var ex in e.LoaderExceptions) {
								Log.Error(ex);
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
			#endregion

			#region Database
			DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			DatabaseManager.DatabaseClosed += OnDatabaseClosed;
			DatabaseManager.DatabaseError += OnDatabaseError;
			DatabaseManager.OpenDatabase();
			#endregion
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
			HttpServer.Instance?.Stop();
			SocketServer?.Shutdown();

			foreach (var service in Services.Values) {
				service.StopService();
			}

			foreach (var plugin in Plugins) {
				plugin.OnStop();
			}

			DatabaseManager.DisposeDatabase();
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

		#region Database Callbacks
		private void OnDatabaseOpened(object sender, EventArgs args) {
			ClientCallback?.OnDatabaseOpened();
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			ClientCallback?.OnDatabaseClosed();
		}

		private void OnDatabaseError(object sender, ErrorEventArgs args) {
			ClientCallback?.OnDatabaseError(args.GetException());
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
			return DatabaseManager.IsOpen;
		}

		public void OpenDatabase() {
			DatabaseManager.OpenDatabase();
		}

		public void DisposeDatabase() {
			DatabaseManager.DisposeDatabase();
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
			var callback = ClientCallback;

			CommandHandler.HandleCommand(command, arguments, default,
			(string output, bool inline) => {
				callback.OnConsoleCommandCallback(output, inline);
			},
			(ParadiseCommand invoker, bool success, string error) => {
				if (success && string.IsNullOrWhiteSpace(error)) {
					//Console.WriteLine(invoker.Output);
				} else {
					Console.WriteLine(error);
				}
			});
		}

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
				DatabaseOpened = this.IsDatabaseOpen(),
				FileServerRunning = this.IsHttpServerRunning()
			};
		}
		#endregion

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

	}
}
