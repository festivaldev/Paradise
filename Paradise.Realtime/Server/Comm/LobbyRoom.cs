namespace Paradise.Realtime.Server.Comm {
	public class LobbyRoom {
		public LobbyRoom(CommPeer peer) {
			peer.AddOperationHandler(new LobbyRoomOperationHandler());
		}
	}
}
