using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			//Room.PowerUpManager.RespawnItems();
			Room.SpawnPointManager.Reset();

			foreach (var player in Room.Players) {
				player.State.SetState(PlayerStateId.PrepareForMatch);

				player.PreviousSpawnPoints.Clear();

				Room.PreparePlayer(player);
				Room.SpawnPlayer(player);
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
			Room.PreparePlayer(args.Player);
			Room.SpawnPlayer(args.Player, true);

			args.Player.State.SetState(PlayerStateId.PrepareForMatch);
		}

		private void OnPlayerLeft(object sender, PlayerLeftEventArgs args) {
			if (!Room.CanStartMatch) {
				MatchStartCountdown?.Stop();
				Room.State.SetState(GameStateId.WaitingForPlayers);
			}
		}
		#endregion
	}
}
