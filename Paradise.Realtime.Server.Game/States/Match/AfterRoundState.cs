using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	internal class AfterRoundState : BaseMatchState {
		public AfterRoundState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
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

			ApplicationConfigurationView appConfig = new ApplicationWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetConfigurationData("4.7.1");

			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendMatchEnd(new EndOfMatchData {
					PlayerStatsTotal = peer.Actor.MatchStatistics,
					PlayerStatsBestPerLife = peer.Actor.MatchStatistics,
					MostEffecientWeaponId = 0,
					PlayerXpEarned = null,
					MostValuablePlayers = MostValuablePlayers.OrderByDescending(_ => _.Kills).ToList(),
					MatchGuid = Room.MetaData.Guid,
					HasWonMatch = false,
					TimeInGameMinutes = (int)TimeSpan.FromMilliseconds(Room.RoundEndTime - Room.RoundStartTime).TotalSeconds
				});
			}

			foreach (var peer in Room.Peers) {

				foreach (var player in Room.Players) {
					if (player.Actor.Info.Cmid == peer.Actor.Info.Cmid) continue;

					player.GameEvents.SendPlayerLeftGame(peer.Actor.Cmid);
				}
			}

			Room.Reset();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
