using Paradise.Core.Models;

namespace Paradise.Realtime.Server.Game {
	internal class WaitingForPlayersState : BaseMatchState {
		public WaitingForPlayersState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;

			Room.WinningTeam = TeamID.NONE;
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }

		#region Handlers
		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			var player = args.Player;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendPlayerJoinedGame(player.Actor.Info, player.Actor.Movement);
			}

			if (Room.CanStartMatch) {
				Room.State.SetState(GameStateId.PrepareNextRound);
			} else {
				player.GameEvents.SendWaitingForPlayers();
			}
		}
		#endregion
	}
}
