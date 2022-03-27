namespace Paradise.Realtime.Server.Game {
	public class WaitingForPlayersPlayerState : PlayerState {
		public WaitingForPlayersPlayerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.Actor.Info.Kills = 0;
			Peer.Actor.Info.Deaths = 0;

			Peer.GameEvents.SendWaitingForPlayers();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}