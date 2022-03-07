using Photon.SocketServer;

namespace Paradise.Realtime.Server.Comm {
	public class CommApplication : Application {
		public static new CommApplication Instance => (CommApplication)Application.Instance;

		public LobbyRoomManager Rooms { get; private set; }

		protected override Peer OnCreatePeer(InitRequest initRequest) {
			return new CommPeer(initRequest);
		}

		protected override void OnSetup() {
			Rooms = new LobbyRoomManager();
		}

		protected override void OnTearDown() { }
	}
}
