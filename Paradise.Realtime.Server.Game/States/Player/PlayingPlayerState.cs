namespace Paradise.Realtime.Server.Game {
	public class PlayingPlayerState : PlayerState {
		public PlayingPlayerState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			Peer.GameEvents.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}