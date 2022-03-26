using log4net;
using Paradise.Core.Models;

namespace Paradise.Realtime.Server.Game {
	public class WaitingForPlayersGameState : GameState {
		public WaitingForPlayersGameState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }



		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			var player = e.Player;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerJoinedGame(player.Actor.Info, player.Actor.Movement);
			}

			if (Room.CanStartMatch) {
				Room.State.SetState(GameStateId.Countdown);
			} else {
				player.State.SetState(PlayerStateId.WaitingForPlayers);
			}
		}
	}
}