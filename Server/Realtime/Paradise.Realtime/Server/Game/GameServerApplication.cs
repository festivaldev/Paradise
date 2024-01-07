using log4net;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Paradise.TcpSocket;

namespace Paradise.Realtime.Server.Game {
	internal class GameServerApplication : BaseRealtimeApplication {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(GameServerApplication));

		public static new GameServerApplication Instance => (GameServerApplication)ApplicationBase.Instance;
		public static new ServerType ServerType = ServerType.Game;
		public GameRoomManager RoomManager { get; private set; } = new GameRoomManager();

		protected System.Timers.Timer MonitoringTimer;

		public int Peers {
			get {
				var count = 0;
				foreach (var room in RoomManager.Rooms.Values) {
					count += room.Peers.Count;
				}
				return count;
			}
		}

		public int Players {
			get {
				var count = 0;
				foreach (var room in RoomManager.Rooms.Values) {
					count += room.Players.Count;
				}
				return count;
			}
		}

		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			return new GamePeer(initRequest);
		}

		protected override void OnBeforeSetup() {
			Identifier = Configuration.GameApplicationSettings.ApplicationIdentifier ?? throw new ArgumentNullException(nameof(Configuration.GameApplicationSettings.ApplicationIdentifier));

			Log.Info($"Starting GameServer[{Identifier}]...");
		}

		protected override void OnSetup() {
			MonitoringTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
			MonitoringTimer.Elapsed += delegate {
				PublishMonitoringData();
			};

			Socket = new TcpSocket.SocketClient(Identifier, ServerType.Game, Configuration.GameApplicationSettings.EncryptionPassPhrase);

			Socket.Connected += (sender, e) => {
				Log.Info("Game: CONNECTED TO SOCKET");

				PublishMonitoringData();
				MonitoringTimer.Start();
			};

			Socket.Disconnected += (sender, e) => {
				Log.Info("Game: DISCONNECTED FROM SOCKET");

				Socket.Reconnect(25);
			};

			Socket.DataReceived += (sender, e) => {
				switch (e.Type) {
					case PacketType.OpenRoom:
						break;

					case PacketType.CloseRoom:
						var roomId = (int)e.Data;

						if (RoomManager.TryGetRoom(roomId, out var room)) {
							var peers = new List<GamePeer>(room.Peers);

							foreach (var roomPeer in peers) {
								room.Leave(roomPeer);
								roomPeer.PeerEventSender.SendRoomEnterFailed(string.Empty, 0, "Game has been closed.");
							}

							RoomManager.RemoveRoom(room.RoomId);
						}
						break;
				}
			};

			Task.Run(async () => {
				var host = new Uri(Configuration.MasterServerUrl).Host;
				var tcpAddress = Dns.GetHostAddresses(host).Where(_ => _.AddressFamily == AddressFamily.InterNetwork).First();

				if (tcpAddress != null) {
					await Socket.Connect(tcpAddress, Configuration.TCPCommPort);
				}
			});

			Log.Info($"Started GameServer[{Identifier}].");
		}

		protected override void OnBeforeTearDown() {
			Log.Info($"Stopping GameServer[{Identifier}]...");

			MonitoringTimer?.Stop();
		}

		protected override void OnTearDown() {
			Log.Info($"Stopped GameServer[{Identifier}].");
		}

		private void PublishMonitoringData() {
			Socket?.SendSync(TcpSocket.PacketType.Monitoring, GetStatus());
		}

		private Dictionary<string, object> GetStatus() {
			try {
				return new Dictionary<string, object> {
					["connected_peers"] = Peers,
					["players"] = Players,
					["rooms"] = RoomManager.Rooms.Values.Select(room => {
						return new Dictionary<string, object> {
							["room_id"] = room.RoomId,
							["is_teamgame"] = room.IsTeamGame,
							["metadata"] = room.MetaData,
							["peers"] = room.Peers,
							//["players"] = room.Players,
							["round_number"] = room.RoundNumber,
							["round_start_time"] = room.RoundStartTime,
							["round_end_time"] = room.RoundEndTime,
							["round_ended"] = room.HasRoundEnded,
							["state"] = room.State
						};
					})
				};
			} catch (Exception e) {
				Log.Error(e);

				return new Dictionary<string, object> { ["error"] = e.Message };
			}
		}
	}
}
