using Photon.SocketServer;
using System;

namespace Paradise.Realtime.Server.Comm {
	public class CommPeer : BasePeer {
		public CommActor Actor { get; set; }
		public LobbyRoom Lobby { get; set; }

		public CommPeerEvents PeerEvents { get; private set; }
		public LobbyRoomEvents LobbyEvents => PeerEvents.LobbyEvents;

		public CommPeer(InitRequest initRequest) : base(initRequest) {
			PeerEvents = new CommPeerEvents(this);

			AddOperationHandler(new CommPeerOperationHandler());
		}

		public override void SendHeartbeat(string hash) {
			PeerEvents.SendHeartbeatChallenge(hash);
		}

		public override void SendError(string message = "An error occured that forced UberStrike to halt.") {
			base.SendError(message);
			PeerEvents.SendDisconnectAndDisablePhoton(message);
		}

		public override string ToString() {
			return $"Peer[{Actor?.ActorInfo?.PlayerName}({Actor?.ActorInfo?.Cmid})]";
		}
	}
}
