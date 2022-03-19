using log4net;
using log4net.Appender;
using log4net.Config;
using Newtonsoft.Json;
using Photon.SocketServer;
using System;
using System.IO;
using System.Linq;

namespace Paradise.Realtime.Server {
	public abstract class BaseApplication : ApplicationBase {
		public static new BaseApplication Instance => (BaseApplication)ApplicationBase.Instance;

		protected ILog Log { get; }
		public ApplicationConfiguration Configuration { get; private set; }
		private PeerConfiguration PeerConfiguration;

		protected BaseApplication() {
			Log = LogManager.GetLogger(GetType().Name);
		}

		protected abstract void OnSetup();
		protected abstract void OnTearDown();
		protected abstract PeerBase OnCreatePeer(InitRequest initRequest);

		protected sealed override void Setup() {
			GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(ApplicationPath, "log");

			var configFile = new FileInfo(Path.Combine(BinaryPath, "log4net.config"));
			if (configFile.Exists) {
				XmlConfigurator.ConfigureAndWatch(configFile);
			}

			var logFile = LogManager.GetRepository().GetAppenders().OfType<FileAppender>().FirstOrDefault()?.File;

			if (logFile != null) {
				File.WriteAllText(logFile, "");
			}

			var applicationConfigPath = Path.Combine(BinaryPath, "Paradise.Realtime.Server.json");
			if (!File.Exists(applicationConfigPath)) {
				Log.Info($"{applicationConfigPath} doesn't exist. Loading default configuration instead.");
				Configuration = ApplicationConfiguration.Default;
			} else {
				try {
					var json = File.ReadAllText(applicationConfigPath);
					Configuration = JsonConvert.DeserializeObject<ApplicationConfiguration>(json);
					Configuration.Validate();

					Log.Info($"Loaded application config from {applicationConfigPath}:");
					Log.Info($"\tWebServiceBaseUrl: {Configuration.WebServiceBaseUrl}");
					Log.Info($"\tHeartbeatInterval: {Configuration.HeartbeatInterval}");
					Log.Info($"\tHeartbeatTimeout: {Configuration.HeartbeatTimeout}");
					Log.Info($"\tCompositeHashes: {Configuration.CompositeHashBytes.Count}");
					Log.Info($"\tJunkHashes: {Configuration.JunkHashBytes.Count}");
				} catch (Exception e) {
					Log.Fatal($"Failed to load application configuration from {applicationConfigPath}: {e.Message}");
					throw;
				}
			}

			PeerConfiguration = new PeerConfiguration(
				webServiceBaseUrl: Configuration.WebServiceBaseUrl,
				heartbeatInterval: Configuration.HeartbeatTimeout,
				heartbeatTimeout: Configuration.HeartbeatInterval,
				compositeHashes: Configuration.CompositeHashBytes.AsReadOnly(),
				junkHashes: Configuration.JunkHashBytes.AsReadOnly()
			);

			OnSetup();
		}

		protected sealed override void TearDown() {
			OnTearDown();
		}

		protected sealed override PeerBase CreatePeer(InitRequest initRequest) {
			Log.Info($"Accepted new connection at {initRequest.RemoteIP}:{initRequest.RemotePort}.");

			initRequest.UserData = PeerConfiguration;
			return OnCreatePeer(initRequest);
		}
	}
}
