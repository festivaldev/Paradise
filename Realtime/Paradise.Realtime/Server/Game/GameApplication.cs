using log4net;
using Photon.SocketServer;

namespace Paradise.Realtime.Server.Game {
	public class GameApplication : BaseRealtimeApplication {
		protected static readonly new ILog Log = LogManager.GetLogger("GameLog");

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
		}

		protected override void OnTearDown() {
			Log.Info("Stopped GameServer.");
		}
	}
}
