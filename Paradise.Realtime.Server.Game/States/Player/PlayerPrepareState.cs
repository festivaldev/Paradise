namespace Paradise.Realtime.Server.Game {
	internal class PlayerPrepareState : BasePlayerState {
		public PlayerPrepareState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.Actor.ResetStatistics();
			Peer.GameEvents.SendPrepareNextRound();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
