using log4net;

namespace Paradise.Realtime.Server.Game {
	public class CountdownGamePeerState : GamePeerState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(CountdownGamePeerState));

		public CountdownGamePeerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() { }

		public override void OnResume() { }

		public override void OnExit() { }

		public override void OnUpdate() { }
	}
}
