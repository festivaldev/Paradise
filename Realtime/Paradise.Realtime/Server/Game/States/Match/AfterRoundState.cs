using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	internal class AfterRoundState : BaseMatchState {
		public ApplicationConfigurationView ApplicationConfiguration => XpPointsUtil.Config;

		public AfterRoundState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			List<StatsSummary> MostValuablePlayers = new List<StatsSummary>();

			foreach (var player in Room.Players) {
				Dictionary<byte, ushort> achievements = new Dictionary<byte, ushort>();

				// Most Valuable (Highest KD)
				if (Room.Players.Where(_ => _.Actor.KillDeathRatio > 1).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.KillDeathRatio).First().Actor.Cmid.CompareTo(player.Actor.Cmid) == 0) {
						achievements.Add((byte)AchievementType.MostValuable, Convert.ToUInt16(player.Actor.KillDeathRatio * 10));
					}
				}

				// Most Aggressive (Most Kills Total)
				if (Room.Players.Where(_ => _.Actor.Kills > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.Kills).First().Actor.Cmid.CompareTo(player.Actor.Cmid) == 0) {
						achievements.Add((byte)AchievementType.MostAggressive, Convert.ToUInt16(player.Actor.Kills));
					}
				}

				// Sharpest Shooter (Most Critical Hits)
				if (Room.Players.Where(_ => _.Actor.MatchStatistics.Headshots + _.Actor.MatchStatistics.Nutshots > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.MatchStatistics.Headshots + _.Actor.MatchStatistics.Nutshots).First().Actor.Cmid.CompareTo(player.Actor.Cmid) == 0) {
						achievements.Add((byte)AchievementType.SharpestShooter, (ushort)(player.Actor.MatchStatistics.Headshots + player.Actor.MatchStatistics.Nutshots));
					}
				}

				// Most Trigger Happy (Highest Killstreak)
				if (Room.Players.Where(_ => _.Actor.MatchStatistics.ConsecutiveSnipes > 1).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.MatchStatistics.ConsecutiveSnipes).First().Actor.Cmid.CompareTo(player.Actor.Cmid) == 0) {
						achievements.Add((byte)AchievementType.TriggerHappy, (ushort)player.Actor.MatchStatistics.ConsecutiveSnipes);
					}
				}

				// Hardest Hitter (Highest Damage Dealt)
				if (Room.Players.Where(_ => _.Actor.MatchStatistics.GetDamageDealt() > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.MatchStatistics.GetDamageDealt()).First().Actor.Cmid.CompareTo(player.Actor.Cmid) == 0) {
						achievements.Add((byte)AchievementType.HardestHitter, (ushort)player.Actor.MatchStatistics.GetDamageDealt());
					}
				}

				// Cost Effective (Highest Accuracy)
				if (Room.Players.Where(_ => _.Actor.Accuracy > 0).Count() > 0) {
					if (Room.Players.OrderByDescending(_ => _.Actor.Accuracy).First().Actor.Cmid.CompareTo(player.Actor.Cmid) == 0) {
						achievements.Add((byte)AchievementType.CostEffective, Convert.ToUInt16(player.Actor.Accuracy * 10));
					}
				}

				MostValuablePlayers.Add(new StatsSummary {
					Cmid = player.Actor.Cmid,
					Achievements = achievements,
					Deaths = player.Actor.MatchStatistics.Deaths + player.Actor.MatchStatistics.Suicides,
					Kills = player.Actor.MatchStatistics.GetKills(),
					Level = player.Actor.Info.Level,
					Name = player.Actor.Info.PlayerName,
					Team = player.Actor.Info.TeamID
				});
			}

			foreach (var player in Room.Players) {
				player.Actor.PerLifeStatistics.Add(player.Actor.CurrentLifeStatistics);

				var matchData = new EndOfMatchData {
					PlayerStatsTotal = player.Actor.MatchStatistics,
					PlayerStatsBestPerLife = player.Actor.GetBestPerLifeStatistics(),
					MostEffecientWeaponId = 0,
					PlayerXpEarned = null,
					MostValuablePlayers = MostValuablePlayers.OrderByDescending(_ => _.Kills).ToList(),
					MatchGuid = Room.MetaData.Guid,
					HasWonMatch = Room.IsTeamGame ? player.Actor.Team == Room.WinningTeam : player.Actor.Cmid == Room.WinningCmid,
					TimeInGameMinutes = (int)Room.RoundDurations.Aggregate((sum, duration) => sum.Add(duration)).TotalSeconds
				};

				CalculateXp(matchData);
				CalculatePoints(matchData);

				UserWebServiceClient.Instance.DepositPoints(new PointDepositView {
					Cmid = player.Actor.Cmid,
					DepositDate = DateTime.UtcNow,
					DepositType = PointsDepositType.Game,
					PointDepositId = new Random((int)DateTime.UtcNow.Ticks).Next(1, int.MaxValue),
					Points = matchData.PlayerStatsTotal.Points,
				}, player.AuthToken);

				player.Actor.SaveStatistics(matchData);

				player.GameEvents.SendMatchEnd(matchData);
				player.State.SetState(PlayerStateId.Overview);
			}

			foreach (var peer in Room.Peers) {
				foreach (var player in Room.Players) {
					if (player.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0) continue;

					player.GameEvents.SendPlayerLeftGame(peer.Actor.Cmid);
				}
			}

			Room.Reset();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }



		private void CalculateXp(EndOfMatchData data) {
			if (data.PlayerStatsTotal.GetDamageDealt() > 0) {
				int gainedXp = (!data.HasWonMatch) ? ApplicationConfiguration.XpBaseLoser : ApplicationConfiguration.XpBaseWinner;
				gainedXp += Math.Max(0, data.PlayerStatsTotal.GetKills()) * ApplicationConfiguration.XpKill;
				gainedXp += Math.Max(0, data.PlayerStatsTotal.Nutshots) * ApplicationConfiguration.XpNutshot;
				gainedXp += Math.Max(0, data.PlayerStatsTotal.Headshots) * ApplicationConfiguration.XpHeadshot;
				gainedXp += Math.Max(0, data.PlayerStatsTotal.MeleeKills) * ApplicationConfiguration.XpSmackdown;

				int xpPerMinute = (!data.HasWonMatch) ? ApplicationConfiguration.XpPerMinuteLoser : ApplicationConfiguration.XpPerMinuteWinner;
				gainedXp += (int)Math.Ceiling((float)(data.TimeInGameMinutes / 60 * xpPerMinute));
				gainedXp += ((int)Math.Ceiling((float)(data.TimeInGameMinutes / 60 * xpPerMinute)) * 0 /* CalculateBoost */);

				data.PlayerStatsTotal.Xp = gainedXp;
			}
		}

		private void CalculatePoints(EndOfMatchData data) {
			if (data.PlayerStatsTotal.GetDamageDealt() > 0) {
				int gainedPoints = (!data.HasWonMatch) ? ApplicationConfiguration.PointsBaseLoser : ApplicationConfiguration.PointsBaseWinner;
				gainedPoints += Math.Max(0, data.PlayerStatsTotal.GetKills()) * ApplicationConfiguration.PointsKill;
				gainedPoints += Math.Max(0, data.PlayerStatsTotal.Nutshots) * ApplicationConfiguration.PointsNutshot;
				gainedPoints += Math.Max(0, data.PlayerStatsTotal.Headshots) * ApplicationConfiguration.PointsHeadshot;
				gainedPoints += Math.Max(0, data.PlayerStatsTotal.MeleeKills) * ApplicationConfiguration.PointsSmackdown;

				int pointsPerMinute = (!data.HasWonMatch) ? ApplicationConfiguration.PointsPerMinuteLoser : ApplicationConfiguration.PointsPerMinuteWinner;
				gainedPoints += (int)Math.Ceiling((float)(data.TimeInGameMinutes / 60 * pointsPerMinute));
				gainedPoints += ((int)Math.Ceiling((float)(data.TimeInGameMinutes / 60 * pointsPerMinute)) * 0 /* CalculateBoost */);

				data.PlayerStatsTotal.Points = gainedPoints;
			}
		}
	}
}
