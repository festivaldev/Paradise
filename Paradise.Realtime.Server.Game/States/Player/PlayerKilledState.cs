namespace Paradise.Realtime.Server.Game {
	internal class PlayerKilledState : BasePlayerState {
		private Countdown RespawnCountdown;

		public PlayerKilledState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			RespawnCountdown = new Countdown(Room.Loop, 5, 0);
			RespawnCountdown.Counted += OnRespawnCountdownCounted;
			RespawnCountdown.Completed += OnRespawnCountdownCompleted;

			RespawnCountdown.Restart();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() {
			RespawnCountdown?.Tick();
		}

		#region Handlers
		private void OnRespawnCountdownCounted(int count) {
			Peer.GameEvents.SendPlayerRespawnCountdown((byte)count);
		}

		private void OnRespawnCountdownCompleted() {
			Peer.GameEvents.SendPlayerRespawnCountdown(0);
		}
		#endregion
	}
}
