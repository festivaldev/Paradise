using log4net;
using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class TeamEliminationGameRoom : BaseGameRoom {
		private readonly static ILog Log = LogManager.GetLogger(nameof(TeamEliminationGameRoom));

		private Dictionary<TeamID, int> TeamScores;

		public TeamEliminationGameRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) {
			TeamScores = new Dictionary<TeamID, int> {
				[TeamID.BLUE] = 0,
				[TeamID.RED] = 0
			};
		}

		public override bool CanJoinMatch => State.CurrentStateId != GameStateId.MatchRunning;
		public override bool CanStartMatch => Players.Where(_ => _.Actor.Team == TeamID.BLUE).Count() >= 1 &&
											  Players.Where(_ => _.Actor.Team == TeamID.RED).Count() >= 1;

		public override void GetCurrentScore(out short killsRemaining, out short blueTeamScore, out short redTeamScore) {
			killsRemaining = -1;
			blueTeamScore = (short)TeamScores[TeamID.BLUE];
			redTeamScore = (short)TeamScores[TeamID.RED];
		}

		public override void Reset() {
			TeamScores[TeamID.BLUE] = 0;
			TeamScores[TeamID.RED] = 0;

			base.Reset();
		}

		protected override void OnMatchEnded(EventArgs args) {
			base.OnMatchEnded(args);
		}

		protected override void OnPlayerKilled(PlayerKilledEventArgs args) {
			if (Players.Where(_ => _.Actor.Team == TeamID.RED && _.Actor.Info.IsAlive).Count() == 0) {
				TeamScores[TeamID.BLUE] += 1;
				WinningTeam = TeamID.BLUE;

				HasRoundEnded = true;
			} else if (Players.Where(_ => _.Actor.Team == TeamID.BLUE && _.Actor.Info.IsAlive).Count() == 0) {
				TeamScores[TeamID.RED] += 1;
				WinningTeam = TeamID.RED;

				HasRoundEnded = true;
			}

			foreach (var peer in Peers) {
				peer.GameEvents.SendUpdateRoundScore(RoundNumber, (short)TeamScores[TeamID.BLUE], (short)TeamScores[TeamID.RED]);
			}

			base.OnPlayerKilled(args);
		}
	}
}
