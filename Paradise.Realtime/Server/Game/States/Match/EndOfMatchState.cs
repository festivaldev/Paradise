using Paradise.Core.Types;
using System;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	internal class EndOfMatchState : BaseMatchState {
		public EndOfMatchState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.RoundEndTime = Environment.TickCount;
			Room.RoundDurations.Add(TimeSpan.FromMilliseconds(Room.RoundEndTime - Room.RoundStartTime));

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

				if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
					short blueTeamScore = 0;
					short redTeamScore = 0;

					Room.GetCurrentScore(out _, out blueTeamScore, out redTeamScore);

					// Game should end if RoundNumber >= KillLimit (aka "Max Rounds")
					if (Math.Max(blueTeamScore, redTeamScore) >= Room.MetaData.KillLimit || !Room.CanStartMatch) {
						Room.State.SetState(GameStateId.AfterRound);
					} else {
						Room.State.SetState(GameStateId.WaitingForPlayers);
					}
				} else {
					Room.State.SetState(GameStateId.AfterRound);
				}
			});
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
