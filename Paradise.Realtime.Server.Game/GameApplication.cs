using Photon.SocketServer;

namespace Paradise.Realtime.Server.Game {
	public class GameApplication : BaseApplication {
		public static new GameApplication Instance => (GameApplication)ApplicationBase.Instance;

		public GameRoomManager RoomManager { get; private set; } = new GameRoomManager();

		public int Peers {
			get {
				var count = 0;
				foreach (var room in RoomManager.Rooms) {
					count += room.Value.Peers.Count;
				}
				return count;
			}
		}

		public int Players {
			get {
				var count = 0;
				foreach (var room in RoomManager.Rooms) {
					count += room.Value.Players.Count;
				}
				return count;
			}
		}

		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			Log.Info($"Accepted new connection at {initRequest.RemoteIP}:{initRequest.RemotePort}");

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