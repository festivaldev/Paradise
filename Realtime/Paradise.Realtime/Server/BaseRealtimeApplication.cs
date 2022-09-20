using log4net;
using log4net.Config;
using Photon.SocketServer;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Paradise.Realtime.Server {
	public abstract class BaseRealtimeApplication : ApplicationBase {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(BaseRealtimeApplication));

		public static new BaseRealtimeApplication Instance => (BaseRealtimeApplication)ApplicationBase.Instance;

		public ApplicationConfiguration Configuration { get; private set; }
		private PeerConfiguration PeerConfiguration;

		protected abstract void OnSetup();
		protected abstract void OnTearDown();
		protected abstract PeerBase OnCreatePeer(InitRequest initRequest);

		protected sealed override void Setup() {
			GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(ApplicationPath, "logs");

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Paradise.Realtime.log4net.config")) {
				using (StreamReader reader = new StreamReader(stream)) {
					var logConfig = new XmlDocument();
					logConfig.LoadXml(reader.ReadToEnd());

					XmlConfigurator.Configure(logConfig.DocumentElement);
				}
			}

			XmlSerializer serializer = new XmlSerializer(typeof(ApplicationConfiguration));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(BinaryPath, "Paradise.Realtime.xml")))) {
					Configuration = (ApplicationConfiguration)serializer.Deserialize(reader);
				}
			} catch (Exception e) {
				Log.Error($"There was an error parsing the settings file: {e.Message}");
				Log.Debug(e);

				Configuration = ApplicationConfiguration.Default;
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
			Log.Debug($"Accepted new connection at {initRequest.RemoteIP}:{initRequest.RemotePort}.");

			initRequest.UserData = PeerConfiguration;
			return OnCreatePeer(initRequest);
		}
	}
}
