﻿using log4net;
using Paradise.WebServices.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.WebServices {
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
	public partial class ParadiseService : ServiceBase, IServiceCallback, IParadiseServiceHost {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseService));

		public static ParadiseService Instance { get; private set; }

		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		private static BasicHttpBinding HttpBinding;
		public Dictionary<string, BaseWebService> Services { get; private set; } = new Dictionary<string, BaseWebService>();

		public static ParadiseServerSettings WebServiceSettings { get; private set; }
		private SimpleHTTPServer HttpServer;

		private IParadiseServiceClient ClientCallback {
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
				ClientCallback?.OnError("There was an error parsing the settings file.", e.Message);
			}

			CommandHandler.CommandCallback += (sender, callbackArgs) => {
				ClientCallback?.OnConsoleCommandCallback(callbackArgs.CommandOutput);
			};

			DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			DatabaseManager.DatabaseClosed += OnDatabaseClosed;
			DatabaseManager.OpenDatabase();

			HttpServer = new SimpleHTTPServer(Path.Combine(CurrentDirectory, WebServiceSettings.FileServerRoot), WebServiceSettings);

			try {
				HttpServer.Start();

				ClientCallback?.OnFileServerStarted();
			} catch (Exception e) {
				Log.Error(e);
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

			Services = new Dictionary<string, BaseWebService> {
				["Application"] = new ApplicationWebService(HttpBinding, WebServiceSettings, this),
				["Authentication"] = new AuthenticationWebService(HttpBinding, WebServiceSettings, this),
				["Clan"] = new ClanWebService(HttpBinding, WebServiceSettings, this),
				["Moderation"] = new ModerationWebService(HttpBinding, WebServiceSettings, this),
				["PrivateMessage"] = new PrivateMessageWebService(HttpBinding, WebServiceSettings, this),
				["Relationship"] = new RelationshipWebService(HttpBinding, WebServiceSettings, this),
				["Shop"] = new ShopWebService(HttpBinding, WebServiceSettings, this),
				["User"] = new UserWebService(HttpBinding, WebServiceSettings, this)
			};

			foreach (var service in Services.Values) {
				service.StartService();
			}
		}

		protected override void OnShutdown() {
			base.OnShutdown();

			SimpleHTTPServer.Instance.Stop();

			foreach (var service in Services.Values) {
				service.StopService();
			}

			DatabaseManager.DisposeDatabase();
			Thread.Sleep(300);
		}

		protected override void OnStop() {
			base.OnStop();

			SimpleHTTPServer.Instance.Stop();

			foreach (var service in Services.Values) {
				service.StopService();
			}

			DatabaseManager.DisposeDatabase();
			Thread.Sleep(300);
		}

		#region Database Callbacks
		private void OnDatabaseOpened(object sender, EventArgs args) {
			ClientCallback?.OnDatabaseOpened();
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			ClientCallback?.OnDatabaseClosed();
		}

		//private void OnDatabaseError(object sender, ErrorEventArgs args) {
		//	Log.Error(args.GetException());
		//	notifyIcon.ShowBalloonTip(3000, "Database error", args.GetException().Message, ToolTipIcon.Error);
		//	notifyIcon.BalloonTipClicked += OpenLogMenuItemClicked;
		//}
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
			if (!DatabaseManager.IsOpen) {
				DatabaseManager.OpenDatabase();
			}
		}

		public void DisposeDatabase() {
			if (DatabaseManager.IsOpen) {
				DatabaseManager.DisposeDatabase();
			}
		}

		public bool IsFileServerRunning() {
			return SimpleHTTPServer.Instance.IsRunning;
		}

		public void StartFileServer() {
			if (!SimpleHTTPServer.Instance.IsRunning) {
				try {
					SimpleHTTPServer.Instance.Start();

					ClientCallback?.OnFileServerStarted();
				} catch (Exception e) {
					Log.Error(e);
					ClientCallback?.OnFileServerError(e);
				}
			}
		}

		public void StopFileServer() {
			if (SimpleHTTPServer.Instance.IsRunning) {
				try {
					SimpleHTTPServer.Instance.Stop();

					ClientCallback?.OnFileServerStopped();
				} catch (Exception e) {
					Log.Error(e);
					ClientCallback?.OnFileServerError(e);
				}
			}
		}

		public void SendConsoleCommand(string command, string[] arguments) {
			Console.WriteLine(string.Join(" ", command, string.Join(" ", arguments)));
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
				DatabaseOpened = DatabaseManager.IsOpen,
				FileServerRunning = this.IsFileServerRunning()
			};
		}
	}
}