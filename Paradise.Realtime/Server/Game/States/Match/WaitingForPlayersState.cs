using Paradise.Core.Models;
using Paradise.Core.Types;

namespace Paradise.Realtime.Server.Game {
	internal class WaitingForPlayersState : BaseMatchState {
		public WaitingForPlayersState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;

			Room.WinningCmid = 0;
			Room.WinningTeam = TeamID.NONE;

			Room.SpawnPointManager.Reset();

			if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
				foreach (var player in Room.Players) {
					Room.PreparePlayer(player);
					Room.SpawnPlayer(player, false);

					player.GameEvents.SendWaitingForPlayers();
				}

				if (Room.CanStartMatch) {
					Room.State.SetState(GameStateId.PrepareNextRound);
				}
			}
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }

		#region Handlers
		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			var player = args.Player;

			Room.PreparePlayer(player);
			Room.SpawnPlayer(player, true);

			if (Room.CanStartMatch) {
				Room.State.SetState(GameStateId.PrepareNextRound);
			} else {
				player.GameEvents.SendWaitingForPlayers();
			}
		}
		#endregion
	}
}
