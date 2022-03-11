using log4net;
using Photon.SocketServer;

namespace Paradise.Realtime.Server.Comm {
	public class CommPeer : BasePeer {
		private static readonly ILog Log = LogManager.GetLogger(typeof(CommPeer));

		public CommActor Actor { get; set; }

		public CommPeerEvents Events { get; private set; }
		public LobbyRoom Lobby { get; private set; }

		public CommPeer(InitRequest initRequest) : base(initRequest) {
			Lobby = new LobbyRoom(this);
			Events = new CommPeerEvents(this);

			AddOperationHandler(new CommPeerOperationHandler());
		}

		public override void SendError(string message = "An error occured that forced UberStrike to halt.") {
			base.SendError(message);
			Events.SendDisconnectAndDisablePhoton(message);
		}

		public override string ToString() {
			return $"Peer[{Actor.ActorInfo.PlayerName}({Actor.ActorInfo.Cmid})]";
		}
	}
}
