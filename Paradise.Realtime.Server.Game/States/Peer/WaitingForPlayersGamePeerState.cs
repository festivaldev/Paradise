using log4net;

namespace Paradise.Realtime.Server.Game {
	public class WaitingForPlayersGamePeerState : GamePeerState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(WaitingForPlayersGamePeerState));

		public WaitingForPlayersGamePeerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.Events.Game.SendWaitingForPlayers();
		}

		public override void OnResume() { }

		public override void OnExit() { }

		public override void OnUpdate() { }
	}
}
