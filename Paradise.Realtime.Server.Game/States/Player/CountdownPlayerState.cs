namespace Paradise.Realtime.Server.Game {
	public class CountdownPlayerState : PlayerState {
		public CountdownPlayerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.Actor.Info.Kills = 0;
			Peer.Actor.Info.Deaths = 0;

			Peer.Actor.ResetStatistics();

			Peer.GameEvents.SendPrepareNextRound();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}