using log4net;
using Photon.SocketServer;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static Paradise.WebSocket;

namespace Paradise.Realtime.Server.Comm {
	public class CommServerApplication : BaseRealtimeApplication {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(CommServerApplication));

		public static new CommServerApplication Instance => (CommServerApplication)ApplicationBase.Instance;
		public static new ServerType ServerType = ServerType.Comm;

		protected System.Timers.Timer MonitoringTimer;

		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			//return new CommPeer(initRequest);
			return null;
		}

		protected override void OnBeforeSetup() {
			if (Configuration.CommApplicationSettings.ApplicationIdentifier == null)
				throw new ArgumentNullException(nameof(Configuration.CommApplicationSettings.ApplicationIdentifier));

			Identifier = Configuration.CommApplicationSettings.ApplicationIdentifier;

			Log.Info($"Starting CommServer[{Identifier}]...");
		}

		protected override void OnSetup() {
			MonitoringTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
			MonitoringTimer.Elapsed += delegate {
				//PublishMonitoringData();
			};

			SocketClient = new SocketClient(Identifier, ServerType.Comm, Configuration.CommApplicationSettings.EncryptionPassPhrase);

			SocketClient.Connected += (sender, e) => {
				Log.Info("Comm: CONNECTED TO SOCKET");

				//PublishMonitoringData();
				MonitoringTimer.Start();
			};

			SocketClient.Disconnected += (sender, e) => {
				Log.Info("Comm: DISCONNECTED FROM SOCKET");

				SocketClient.Reconnect(25);
			};

			SocketClient.DataReceived += (sender, e) => { };

			//var host = new Uri(Configuration.MasterHostname).Host;
			var tcpAddress = Dns.GetHostAddresses(Configuration.MasterHostname).Where(_ => _.AddressFamily == AddressFamily.InterNetwork).First();

			if (tcpAddress != null) {
				SocketClient.Connect(tcpAddress, Configuration.SocketPort);
			}

			Log.Info($"Started CommServer[{Identifier}].");
		}

		protected override void OnBeforeTearDown() {
			Log.Info($"Stopping CommServer[{Identifier}]...");

			MonitoringTimer?.Stop();
		}

		protected override void OnTearDown() {
			Log.Info($"Stopped CommServer[{Identifier}].");
		}

		private void PublishMonitoringData() {
			//Socket?.SendSync(PacketType.Monitoring, GetStatus());
		}
	}
}
