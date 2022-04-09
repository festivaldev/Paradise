using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	internal class EndOfMatchState : BaseMatchState {
		public EndOfMatchState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;

			Room.RoundEndTime = Environment.TickCount;

			if (Room.MetaData.GameMode == GameModeType.DeathMatch) {
				short killsRemaining = (short)Room.MetaData.KillLimit;

				Room.GetCurrentScore(out killsRemaining, out _, out _);
				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendKillsRemaining(killsRemaining, 0);
				}
			} else {
				short blueTeamScore = 0;
				short redTeamScore = 0;

				Room.GetCurrentScore(out _, out blueTeamScore, out redTeamScore);
				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
				}
			}

			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendTeamWins(Room.WinningTeam);
			}

			Task t = Task.Run(async () => {
				await Task.Delay(3000);

				Room.State.SetState(GameStateId.AfterRound);
			});
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }

		#region Handlers
		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			// TODO: Determine what to do if a player joins during 3 second timeout
		}
		#endregion
	}
}
