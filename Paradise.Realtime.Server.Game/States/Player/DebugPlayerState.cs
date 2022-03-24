using log4net;

namespace Paradise.Realtime.Server.Game {
	public class DebugPlayerState : PlayerState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(DebugPlayerState));

		public DebugPlayerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Log.Info("entered debug state (peer)");

			Peer.GameEvents.SendWaitingForPlayers();
		}

		public override void OnExit() {
			Log.Info("left debug state (peer)");
		}

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}