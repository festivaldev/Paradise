using log4net;

namespace Paradise.Realtime.Server.Game {
	public class PlayingGamePeerState : GamePeerState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(PlayingGamePeerState));

		public PlayingGamePeerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() { }

		public override void OnResume() { }

		public override void OnExit() { }

		public override void OnUpdate() { }
	}
}
