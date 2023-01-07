using log4net;
using Newtonsoft.Json;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	public class GameApplication : BaseRealtimeApplication {
		protected ServiceHost ServiceHost;

		public static new GameApplication Instance => (GameApplication)ApplicationBase.Instance;

		public GameRoomManager RoomManager { get; private set; } = new GameRoomManager();

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
			Log.Info("Started GameServer.");

			ServiceHost = new ServiceHost(typeof(GameApplicationMonitoring));
			ServiceHost.AddServiceEndpoint(typeof(IParadiseMonitoring), new NetNamedPipeBinding(), "net.pipe://localhost/NewParadise.Monitoring.Game");

			Task.Factory.StartNew(() => {
				Task.Delay(300).Wait();

				ServiceHost.Open();
			});
		}

		protected override void OnTearDown() {
			Log.Info("Stopped GameServer.");

			ServiceHost.Abort();
		}
	}

	public class GameApplicationMonitoring : IParadiseMonitoring {
		protected static readonly new ILog Log = LogManager.GetLogger("GameLog");

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

		public byte Ping() {
			return 0x1;
		}
	}
}
