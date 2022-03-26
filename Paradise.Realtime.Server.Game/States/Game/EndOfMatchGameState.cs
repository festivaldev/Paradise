using Paradise.Core.Models;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class EndOfMatchGameState : GameState {
		private Countdown RestartCountdown;

		public EndOfMatchGameState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			RestartCountdown = new Countdown(Room.Loop, 5, 0);
			RestartCountdown.Completed += OnRestartCountdownCompleted;

			RestartCountdown.Restart();

			foreach (var peer in Room.Peers) {
				//	peer.GameEvents.SendTeamWins(TeamID.BLUE);
				//	peer.GameEvents.SendMatchEnd(new EndOfMatchData {
				//		HasWonMatch = true,
				//		MatchGuid = Room.MetaData.Guid
				//	});

				peer.GameEvents.SendTeamWins(Room.WinningTeam);
				peer.State.SetState(PlayerStateId.AfterRound);
			}

			List<StatsSummary> MostValuablePlayers = new List<StatsSummary>();
			foreach (var mvp in Room.Players) {
				Dictionary<byte, ushort> achievements = new Dictionary<byte, ushort>();
				// Most Valuable (Highest KD)
				// KD is divided by 10 by the client because you cant send decimals through ushort
				//if (Players.MaxBy(x => x.TotalStats.GetKills()).Actor.Cmid == mvp.Actor.Cmid)
				achievements.Add(1, (ushort)(8) * 10);
				// Most Aggressive (Most Kills Total)
				//if (Players.MaxBy(x => x.TotalStats.GetKills()).Actor.Cmid == mvp.Actor.Cmid)
				achievements.Add(2, (ushort)1337);
				// Sharpest Shooter (Most Crits aka Most Headshots and Nutshots)
				//if (Players.MaxBy(x => x.TotalStats.Headshots + x.TotalStats.Nutshots).Actor.Cmid == mvp.Actor.Cmid)
				achievements.Add(3, (ushort)(1337));
				// Most Trigger Happy (Highest Killstreak)
				//if (Players.MaxBy(x => x.TotalStats.ConsecutiveSnipes).Actor.Cmid == mvp.Actor.Cmid)
				achievements.Add(4, (ushort)(1337));
				// Hardest Hitter (Highest Damage Dealt)
				//if (Players.MaxBy(x => x.TotalStats.GetDamageDealt()).Actor.Cmid == mvp.Actor.Cmid)
				achievements.Add(5, (ushort)1337);

				MostValuablePlayers.Add(new StatsSummary {
					Cmid = mvp.Actor.Cmid,
					Achievements = achievements,
					Deaths = 1337,
					Kills = 1337,
					Level = 1337,
					Name = "debug",
					Team = TeamID.NONE
				});
			}

			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendMatchEnd(new EndOfMatchData {
					PlayerStatsTotal = new StatsCollection(),
					PlayerStatsBestPerLife = new StatsCollection(),
					MostEffecientWeaponId = 0,
					PlayerXpEarned = null,
					MostValuablePlayers = MostValuablePlayers,
					MatchGuid = Room.MetaData.Guid,
					HasWonMatch = false,
					TimeInGameMinutes = 300
				});
			}
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() {
			RestartCountdown.Tick();
		}



		private void OnRestartCountdownCompleted() {
			Room.Reset();
		}
	}
}