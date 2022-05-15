using log4net;
using log4net.Config;
using Paradise.WebServices.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.WebServices.ServiceHost {
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
	public partial class ParadiseService : ServiceBase, IServiceCallback, IParadiseServiceHost {
		protected static readonly ILog Log = LogManager.GetLogger(typeof(ParadiseService));

		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		private static BasicHttpBinding HttpBinding;
		static Dictionary<string, WebServiceBase> Services = new Dictionary<string, WebServiceBase>();
		
		private ParadiseSettings WebServiceSettings;
		private SimpleHTTPServer HttpServer;

		private IParadiseServiceClient ClientCallback {
			get {
				return OperationContext.Current?.GetCallbackChannel<IParadiseServiceClient>();
			}
		}

		public ParadiseService() {
			InitializeComponent();
		}

		protected override void OnStart(string[] args) {
			var configFile = new FileInfo(Path.Combine(CurrentDirectory, "log4net.config"));
			if (configFile.Exists) {
				XmlConfigurator.ConfigureAndWatch(configFile);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(ParadiseSettings));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(CurrentDirectory, "ParadiseSettings.Server.xml")))) {
					WebServiceSettings = (ParadiseSettings)serializer.Deserialize(reader);
				}
			} catch (Exception e) {
				Log.Error(e);
				ClientCallback?.OnError("There was an error parsing the settings file.", e.Message);
			}

			DatabaseManager.DatabaseOpened += OnDatabaseOpened;
			DatabaseManager.DatabaseClosed += OnDatabaseClosed;
			DatabaseManager.OpenDatabase();

			HttpBinding = new BasicHttpBinding();

			if (WebServiceSettings.EnableSSL) {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
				HttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
			} else {
				HttpBinding.Security.Mode = BasicHttpSecurityMode.None;
			}

			Services = new Dictionary<string, WebServiceBase> {
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

			HttpServer = new SimpleHTTPServer(Path.Combine(CurrentDirectory, "www"), WebServiceSettings);
			Log.Info($"http server: {SimpleHTTPServer.Instance}");

			try {
				SimpleHTTPServer.Instance.Start();

				ClientCallback?.OnFileServerStarted();
			} catch (Exception e) {
				Log.Error(e);
			}
		}

		protected override void OnStop() {
			SimpleHTTPServer.Instance.Stop();

			foreach (var service in Services.Values) {
				service.StopService();
			}

			DatabaseManager.DisposeDatabase();
		}

		#region Database Callbacks
		private void OnDatabaseOpened(object sender, EventArgs args) {
			Log.Warn("database is open");
			ClientCallback?.OnDatabaseOpened();
		}

		private void OnDatabaseClosed(object sender, EventArgs args) {
			Log.Warn("database is close");
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
			Log.Info(serviceName);
			Log.Info(serviceVersion);

			if (Services.ContainsKey(serviceName)) {
				return Services[serviceName].StartService();
			} else if (Services.ContainsKey(serviceName.Replace("WebService", ""))) {
				return Services[serviceName.Replace("WebService", "")].StartService();
			}

			return false;
		}

		public bool StopService(string serviceName, string serviceVersion) {
			if (Services.ContainsKey(serviceName)) {
				return Services[serviceName].StopService();
			} else if (Services.ContainsKey(serviceName.Replace("WebService", ""))) {
				return Services[serviceName.Replace("WebService", "")].StopService();
			}

			return false;
		}

		public bool RestartService(string serviceName, string serviceVersion) {
			if (Services.ContainsKey(serviceName)) {
				if (!Services[serviceName].StopService()) return false;

				return Services[serviceName].StartService();
			}

			return false;
		}

		public void StartAllServices() {
			foreach (var service in Services.Values) {
				service.StartService();
			}
		}

		public void StopAllServices() {
			foreach (var service in Services.Values) {
				service.StopService();
			}
		}

		public void RestartAllServices() {
			foreach (var service in Services.Values) {
				service.StopService();

				if (!service.StartService()) {
					ClientCallback?.OnError("Service Restart Failed", $"Service {service.ServiceName} [{service.ServiceVersion}] failed to restart. Please check the log file for further information.");
				}
			}
		}

		public bool IsAnyServiceRunning() {
			return Services.Values.Any(_ => _.IsRunning);
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
		#endregion

		public ParadiseServiceStatus UpdateClientInfo() {
			var services = new List<Dictionary<string, object>>();

			foreach (var service in Services) {
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
