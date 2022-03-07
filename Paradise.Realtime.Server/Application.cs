using log4net;
using log4net.Config;
using Photon.SocketServer;
using System.IO;

namespace Paradise.Realtime.Server {
	public abstract class Application : ApplicationBase {
		public static new Application Instance => (Application)ApplicationBase.Instance;

		protected ILog Log { get; }
		public ApplicationConfiguration Configuration { get; private set; }
		private PeerConfiguration PeerConfig { get; set; }

		protected Application() {
			Log = LogManager.GetLogger(GetType().Name);
		}

		protected abstract void OnSetup();
		protected abstract void OnTearDown();
		protected abstract Peer OnCreatePeer(InitRequest initRequest);

		protected sealed override PeerBase CreatePeer(InitRequest initRequest) {
			Log.Info($"Accepted new connection at {initRequest.RemoteIP}:{initRequest.RemotePort}.");

			initRequest.UserData = PeerConfig;
			return OnCreatePeer(initRequest);
		}

		protected sealed override void Setup() {
			GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(ApplicationRootPath, "log");

			var file = new FileInfo(Path.Combine(BinaryPath, "log4net.config"));
			if (file.Exists) {
				XmlConfigurator.ConfigureAndWatch(file);
			}

			Configuration = ApplicationConfiguration.Default;

			Log.Info(Configuration);
			PeerConfig = new PeerConfiguration (
				Configuration.ServiceBaseURL,
				Configuration.ServiceAuth,
				Configuration.HeartbeatTimeout,
				Configuration.HeartbeatInterval,
				Configuration.CompositeHashBytes,
				Configuration.JunkHashBytes
			);

			OnSetup();
		}

		protected sealed override void TearDown() {
			OnTearDown();
		}
	}
}
