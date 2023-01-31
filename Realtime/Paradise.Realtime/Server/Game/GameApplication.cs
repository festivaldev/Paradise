using log4net;
using Newtonsoft.Json;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class GameApplication : BaseRealtimeApplication {
		protected static readonly new ILog Log = LogManager.GetLogger("GameLog");

		public static new GameApplication Instance => (GameApplication)ApplicationBase.Instance;

		public string Identifier = Guid.NewGuid().ToString();
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

		protected override void OnSetup() {
			MonitoringTimer = new System.Timers.Timer(5000);
			MonitoringTimer.Elapsed += delegate {
				ApplicationWebServiceClient.Instance.PublishGameMonitoringData(Identifier, GetStatus());
			};
			MonitoringTimer.Start();

			Log.Info($"Started GameServer[{Identifier}].");
		}

		protected override void OnTearDown() {
			Log.Info($"Stopping GameServer[{Identifier}]...");

			MonitoringTimer.Stop();
		}

		public string GetStatus() {
			try {
				return JsonConvert.SerializeObject(new Dictionary<string, object> {
					["connected_peers"] = GameApplication.Instance.Peers,
					["players"] = GameApplication.Instance.Players,
					["rooms"] = GameApplication.Instance.RoomManager.Rooms.Values.Select(room => {
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
				});
			} catch (Exception e) {
				Log.Error(e);

				return $"{{\"error\": \"{e.Message}\"}}";
			}
		}
	}
}
