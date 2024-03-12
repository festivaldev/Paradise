using Photon.SocketServer;

namespace Paradise.Realtime.Server.Comm {
	public partial class CommPeer : BasePeer {
		public CommActor Actor { get; set; }
		public LobbyRoom Lobby { get; set; }

		public CommPeer.EventSender PeerEventSender { get; private set; }
		public LobbyRoom.EventSender LobbyEventSender => PeerEventSender.LobbyEventSender;

		private static readonly OperationHandler OpHandler = new OperationHandler();

		public CommPeer(InitRequest initRequest) : base(initRequest) {
			PeerEventSender = new EventSender(this);

			AddOperationHandler(OpHandler);
		}

		protected override void SendHeartbeat(string hash) {
			PeerEventSender.SendHeartbeatChallenge(hash);
		}

		public override void SendError(string message = "An error occured that forced UberStrike to halt.") {
			base.SendError(message);
			PeerEventSender.SendDisconnectAndDisablePhoton(message);
		}

		public override string ToString() {
			return $"CommPeer[{Actor?.ActorInfo?.PlayerName}({Actor?.ActorInfo?.Cmid})]";
		}
	}
}
