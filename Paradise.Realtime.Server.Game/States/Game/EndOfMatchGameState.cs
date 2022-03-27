using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class EndOfMatchGameState : GameState {
		private Countdown RestartCountdown;

		public EndOfMatchGameState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			RestartCountdown = new Countdown(Room.Loop, 5, 0);
			RestartCountdown.Completed += OnRestartCountdownCompleted;

			RestartCountdown.Restart();

			List<StatsSummary> MostValuablePlayers = new List<StatsSummary>();
			foreach (var player in Room.Players) {
				Dictionary<byte, ushort> achievements = new Dictionary<byte, ushort>();

				// Most Valuable (Highest KD)
				if (Room.Players.Where(_ => _.Actor.KillDeathRatio > 1).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.KillDeathRatio).First().Actor.Cmid == player.Actor.Cmid) {
						achievements.Add((byte)AchievementType.MostValuable, Convert.ToUInt16(player.Actor.KillDeathRatio * 10));
					}
				}

				// Most Aggressive (Most Kills Total)
				if (Room.Players.Where(_ => _.Actor.Kills > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.Kills).First().Actor.Cmid == player.Actor.Cmid) {
						achievements.Add((byte)AchievementType.MostAggressive, Convert.ToUInt16(player.Actor.Kills));
					}
				}

				// Sharpest Shooter (Most Critical Hits)
				if (Room.Players.Where(_ => _.Actor.MatchStatistics.Headshots + _.Actor.MatchStatistics.Nutshots > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.MatchStatistics.Headshots + _.Actor.MatchStatistics.Nutshots).First().Actor.Cmid == player.Actor.Cmid) {
						achievements.Add((byte)AchievementType.SharpestShooter, (ushort)(player.Actor.MatchStatistics.Headshots + player.Actor.MatchStatistics.Nutshots));
					}
				}

				// Most Trigger Happy (Highest Killstreak)
				if (Room.Players.Where(_ => _.Actor.MatchStatistics.ConsecutiveSnipes > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.MatchStatistics.ConsecutiveSnipes).First().Actor.Cmid == player.Actor.Cmid) {
						achievements.Add((byte)AchievementType.TriggerHappy, (ushort)player.Actor.MatchStatistics.ConsecutiveSnipes);
					}
				}

				// Hardest Hitter (Highest Damage Dealt)
				if (Room.Players.Where(_ => _.Actor.MatchStatistics.GetDamageDealt() > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.MatchStatistics.GetDamageDealt()).First().Actor.Cmid == player.Actor.Cmid) {
						achievements.Add((byte)AchievementType.HardestHitter, (ushort)player.Actor.MatchStatistics.GetDamageDealt());
					}
				}

				// Cost Effective (Highest Accuracy)
				if (Room.Players.Where(_ => _.Actor.Accuracy > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.Accuracy).First().Actor.Cmid == player.Actor.Cmid) {
						achievements.Add((byte)AchievementType.CostEffective, Convert.ToUInt16(player.Actor.Accuracy * 10));
					}
				}

				MostValuablePlayers.Add(new StatsSummary {
					Cmid = player.Actor.Cmid,
					Achievements = achievements,
					Deaths = player.Actor.Info.Deaths,
					Kills = player.Actor.Info.Kills,
					Level = player.Actor.Info.Level,
					Name = player.Actor.Info.PlayerName,
					Team = player.Actor.Info.TeamID
				});
			}

			var endTime = Environment.TickCount;

			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendMatchEnd(new EndOfMatchData {
					PlayerStatsTotal = new StatsCollection(),
					PlayerStatsBestPerLife = new StatsCollection(),
					MostEffecientWeaponId = 0,
					PlayerXpEarned = null,
					MostValuablePlayers = MostValuablePlayers,
					MatchGuid = Room.MetaData.Guid,
					HasWonMatch = false,
					TimeInGameMinutes = (int)TimeSpan.FromMilliseconds(endTime - Room.RoundStartTime).TotalSeconds
				});
			}

			foreach (var peer in Room.Peers) {
				peer.Actor.Info.PlayerState &= ~PlayerStates.Shooting;

				foreach (var player in Room.Players) {
					if (player.Actor.Info.Cmid == peer.Actor.Info.Cmid) continue;

					player.GameEvents.SendPlayerLeftGame(peer.Actor.Cmid);
				}
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