using System;

namespace Paradise.Realtime.Server.Game {
	internal class PlayerKilledState : BasePlayerState {
		private Countdown RespawnCountdown;

		public PlayerKilledState(GamePeer peer) : base(peer) { }

		public override void OnEnter() {
			// Only allow respawn when not in Team Elimination
			if (Peer.Room.MetaData.GameMode != Core.Types.GameModeType.EliminationMode) {
				Peer.Actor.NextRespawnTime = DateTime.UtcNow.AddSeconds(5);

				RespawnCountdown = new Countdown(Room.Loop, 5, 0);
				RespawnCountdown.Counted += OnRespawnCountdownCounted;
				RespawnCountdown.Completed += OnRespawnCountdownCompleted;

				RespawnCountdown.Restart();
			}
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() {
			RespawnCountdown?.Tick();
		}

		#region Handlers
		private void OnRespawnCountdownCounted(int count) {
			Peer.GameEventSender.SendPlayerRespawnCountdown((byte)count);
		}

		private void OnRespawnCountdownCompleted() {
			Peer.GameEventSender.SendPlayerRespawnCountdown(0);
		}
		#endregion
	}
}
