using log4net;

namespace Paradise.Realtime.Server.Game {
	public class OverviewGamePeerState : GamePeerState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(OverviewGamePeerState));

		public OverviewGamePeerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() { }

		public override void OnResume() { }

		public override void OnExit() { }

		public override void OnUpdate() { }
	}
}
