using log4net;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Paradise.TcpSocket;

namespace Paradise.Realtime.Server.Comm {
	public class CommServerApplication : BaseRealtimeApplication {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(CommServerApplication));

		public static new CommServerApplication Instance => (CommServerApplication)ApplicationBase.Instance;
		public static new ServerType ServerType = ServerType.Comm;

		protected System.Timers.Timer MonitoringTimer;

		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			return new CommPeer(initRequest);
		}

		protected override void OnBeforeSetup() {
			Identifier = Configuration.CommApplicationSettings.ApplicationIdentifier ?? throw new ArgumentNullException(nameof(Configuration.CommApplicationSettings.ApplicationIdentifier));

			Log.Info($"Starting CommServer[{Identifier}]...");
		}

		protected override void OnSetup() {
			MonitoringTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
			MonitoringTimer.Elapsed += delegate {
				PublishMonitoringData();
			};

			Socket = new SocketClient(Identifier, ServerType.Comm, Configuration.CommApplicationSettings.EncryptionPassPhrase);

			Socket.Connected += (sender, e) => {
				Log.Info("Comm: CONNECTED TO SOCKET");

				PublishMonitoringData();
				MonitoringTimer.Start();
			};

			Socket.Disconnected += (sender, e) => {
				Log.Info("Comm: DISCONNECTED FROM SOCKET");

				Socket.Reconnect(0);
			};

			Socket.DataReceived += (sender, e) => {
				switch (e.Type) {
					case PacketType.ChatMessage:
						var message = (SocketChatMessage)e.Data;

						foreach (var peer in LobbyManager.Instance.Peers) {
							peer.LobbyEventSender.SendLobbyChatMessage(message.Cmid, message.Name, message.Message);
						}
						break;
					//case PayloadType.PlayerList:
					//	Socket.SendSync(PayloadType.PlayerList, LobbyManager.Instance.Peers.Select(peer => peer.Actor.ActorInfo).ToList(), true, e.Payload.ConversationId);
					//	break;
					case PacketType.BanPlayer: {
						var data = (Dictionary<string, object>)e.Data;
						var targetPeer = LobbyManager.Instance.Peers.FirstOrDefault(_ => _.Actor.Cmid == (long)data["TargetCmid"]);

						if (targetPeer != null) {
							if ((long)data["Duration"] == 0) {
								targetPeer.SendError($"You have been banned permanently.\n\nReason: {data["Reason"]}");
							} else {
								var expireTime = ((DateTime)data["ExpireTime"]).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss \"GMT\"zzz");
								targetPeer.SendError($"You have been banned for {data["Duration"]} minute(s).\nYour ban will expire at {expireTime}\n\nReason: {data["Reason"]}");
							}
						}

						break;
					}
				}
			};

			Task.Run(async () => {
				var host = new Uri(Configuration.MasterServerUrl).Host;
				var tcpAddress = Dns.GetHostAddresses(host).Where(_ => _.AddressFamily == AddressFamily.InterNetwork).First();

				if (tcpAddress != null) {
					await Socket.Connect(tcpAddress, Configuration.TCPCommPort);
				}
			});

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
			Socket?.SendSync(PacketType.Monitoring, GetStatus());
		}

		private Dictionary<string, object> GetStatus() {
			try {
				return new Dictionary<string, object> {
					["peers"] = LobbyManager.Instance.Peers.Select(peer => peer.Actor.ActorInfo),
					["updated_at"] = DateTime.UtcNow.ToString("o")
				};
			} catch (Exception e) {
				Log.Error(e);

				return new Dictionary<string, object> { ["error"] = e.Message };
			}
		}
	}
}
