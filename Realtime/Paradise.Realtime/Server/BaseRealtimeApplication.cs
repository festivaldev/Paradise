using log4net;
using log4net.Config;
using Paradise.DataCenter.Common.Entities;
using Photon.SocketServer;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using static Paradise.TcpSocket;

namespace Paradise.Realtime.Server {
	public abstract class BaseRealtimeApplication : ApplicationBase {
		protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseRealtimeApplication));

		public static new BaseRealtimeApplication Instance => (BaseRealtimeApplication)ApplicationBase.Instance;
		public static ServerType ServerType;

		public string EncryptionPassPhrase { get; private set; }
		public string EncryptionInitVector { get; private set; }

		public string Identifier;
		public SocketClient Socket;

		public ApplicationConfiguration Configuration { get; private set; }
		private PeerConfiguration PeerConfiguration;

		protected virtual void OnBeforeSetup() { }
		protected virtual void OnSetup() { }
		protected virtual void OnBeforeTearDown() { }
		protected virtual void OnTearDown() { }

		protected abstract PeerBase OnCreatePeer(InitRequest initRequest);

		protected sealed override void Setup() {
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
				Log.Error(e.ExceptionObject);
				HandleException(e.ExceptionObject as Exception);
			};

			GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(ApplicationPath, "logs");

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Paradise.Realtime.log4net.config")) {
				using (StreamReader reader = new StreamReader(stream)) {
					var logConfig = new XmlDocument();
					logConfig.LoadXml(reader.ReadToEnd());

					XmlConfigurator.Configure(logConfig.DocumentElement);
				}
			}

			XmlSerializer appConfigSerializer = new XmlSerializer(typeof(ApplicationConfiguration));
			try {
				using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(BinaryPath, "Paradise.Realtime.xml")), new XmlReaderSettings { IgnoreComments = true })) {
					Configuration = (ApplicationConfiguration)appConfigSerializer.Deserialize(reader);

					if (File.Exists(Path.Combine(BinaryPath, "CompositeHashes.txt"))) {
						Configuration.CompositeHashes = File.ReadAllLines(Path.Combine(BinaryPath, "CompositeHashes.txt")).ToList();
					}

					if (File.Exists(Path.Combine(BinaryPath, "JunkHashes.txt"))) {
						Configuration.JunkHashes = File.ReadAllLines(Path.Combine(BinaryPath, "JunkHashes.txt")).ToList();
					}

					Configuration.Validate();
				}
			} catch (Exception e) {
				Log.Error($"There was an error parsing the settings file: {e.Message}");
				Log.Debug(e);

				Configuration = ApplicationConfiguration.Default;
			}

			OnBeforeSetup();

			PeerConfiguration = new PeerConfiguration(
				heartbeatInterval: Configuration.HeartbeatTimeout,
				heartbeatTimeout: Configuration.HeartbeatInterval,
				compositeHashes: Configuration.CompositeHashBytes.AsReadOnly(),
				junkHashes: Configuration.JunkHashBytes.AsReadOnly()
			) {
				HashVerificationEnabled = Configuration.EnableHashVerification
			};

			try {
				if (ApplicationWebServiceClient.Instance.AuthenticateApplication("4.7.1", ChannelType.Steam, "paradiserealtime") is var data) {
					EncryptionInitVector = data.EncryptionInitVector;
					EncryptionPassPhrase = data.EncryptionPassPhrase;
				}
			} catch (EndpointNotFoundException) {
				Log.Fatal("Could not connect to the Web Services. Ensure the Web Services are running and restart the Realtime application.");

				return;
			}

			OnSetup();
		}

		protected sealed override void TearDown() {
			OnBeforeTearDown();

			OnTearDown();
		}

		protected sealed override PeerBase CreatePeer(InitRequest initRequest) {
			Log.Info($"Accepted new connection at {initRequest.RemoteIP}:{initRequest.RemotePort}.");

			initRequest.UserData = PeerConfiguration;
			return OnCreatePeer(initRequest);
		}

		public void HandleException(Exception exception) {
			if (Configuration.DiscordErrorLog) {
				Socket?.SendSync(PacketType.Error, new RealtimeError {
					Type = ServerType,
					ExceptionType = exception.GetType(),
					Message = exception.Message,
					StackTrace = exception.StackTrace
				});
			}
		}
	}
}