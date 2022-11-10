using Paradise.Core.Types;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	internal class PrepareNextRoundState : BaseMatchState {
		private Countdown MatchStartCountdown;

		public PrepareNextRoundState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerLeft += OnPlayerLeft;

			Room.HasRoundEnded = false;
			Room.RoundNumber++;

			Room.PowerUpManager.RespawnItems();
			Room.SpawnPointManager.Reset();

			foreach (var player in Room.Players) {
				player.PreviousSpawnPoints.Clear();

				bool wasSpectator = player.Actor.Info.IsSpectator;

				Room.PreparePlayer(player);
				Room.SpawnPlayer(player, wasSpectator);

				player.State.SetState(PlayerStateId.PrepareForMatch);

				if (Room.MetaData.GameMode == GameModeType.DeathMatch) {
					short killsRemaining = (short)Room.MetaData.KillLimit;

					Room.GetCurrentScore(out killsRemaining, out _, out _);

					player.GameEvents.SendKillsRemaining(killsRemaining, 0);
				} else {
					short blueTeamScore = 0;
					short redTeamScore = 0;

					Room.GetCurrentScore(out _, out blueTeamScore, out redTeamScore);

					player.GameEvents.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
				}
			}

			Task t = Task.Run(async () => {
				await Task.Delay(500);

				MatchStartCountdown = new Countdown(Room.Loop, 5, 0);
				MatchStartCountdown.Counted += OnCountdownCounted;
				MatchStartCountdown.Completed += OnCountdownCompleted;

				MatchStartCountdown.Restart();
			});
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerLeft -= OnPlayerLeft;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			MatchStartCountdown?.Tick();
		}



		#region Handlers
		private void OnCountdownCounted(int count) {
			foreach (var peer in Room.Players) {
				peer.GameEvents.SendMatchStartCountdown((byte)count);
			}
		}

		private void OnCountdownCompleted() {
			Room.State.SetState(GameStateId.MatchRunning);
		}

		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			args.Player.PreviousSpawnPoints.Clear();


			Room.PreparePlayer(args.Player);
			Room.SpawnPlayer(args.Player, true);

			args.Player.GameEvents.SendWaitingForPlayers();
			args.Player.State.SetState(PlayerStateId.PrepareForMatch);
		}

		private void OnPlayerLeft(object sender, PlayerLeftEventArgs args) {
			if (!Room.CanStartMatch) {
				MatchStartCountdown?.Stop();
				//Room.State.SetState(GameStateId.EndOfMatch);
			}
		}
		#endregion
	}
}
