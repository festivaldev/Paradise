﻿using System;
using System.Collections.Generic;
using System.Linq;
using UberStrike.Core.Models;
using UberStrike.Core.Types;

namespace Paradise.Realtime.Server.Game {
	public partial class GameActor {
		public StatsCollection MatchStatistics { get; private set; } = new StatsCollection();
		public StatsCollection CurrentLifeStatistics { get; private set; } = new StatsCollection();
		public List<StatsCollection> PerLifeStatistics { get; private set; } = new List<StatsCollection>();

		public short Kills => (short)MatchStatistics.GetKills();
		public short Deaths => (short)MatchStatistics.Deaths;
		public double KillDeathRatio => Kills / Math.Max(1, (int)Deaths);
		public double Accuracy => Math.Min((MatchStatistics.GetHits() / Math.Max(1, MatchStatistics.GetShots())) * 100, 100);

		public void ResetCurrentLifeStatistics() {
			CurrentLifeStatistics = new StatsCollection();
		}

		public void ResetStatistics() {
			MatchStatistics = new StatsCollection();
			CurrentLifeStatistics = new StatsCollection();
			PerLifeStatistics.Clear();
		}

		public void SaveStatistics(EndOfMatchData matchData) {
			var bestPerLifeStatistics = matchData.PlayerStatsBestPerLife;
			var totalStatistics = matchData.PlayerStatsTotal;

			var statistics = Peer.Member.UberstrikeMemberView.PlayerStatisticsView;

			statistics.Hits += totalStatistics.GetHits();
			statistics.Shots += totalStatistics.GetShots();
			statistics.Splats += totalStatistics.GetKills();
			statistics.Splatted += totalStatistics.Deaths;
			statistics.Headshots += totalStatistics.Headshots;
			statistics.Nutshots += totalStatistics.Nutshots;
			statistics.Xp += totalStatistics.Xp;
			statistics.TimeSpentInGame += matchData.TimeInGameMinutes;
			statistics.Level = XpPointsUtil.GetLevelForXp(statistics.Xp);

			// Machine Gun
			statistics.WeaponStatistics.MachineGunTotalDamageDone += matchData.PlayerStatsTotal.MachineGunDamageDone;
			statistics.WeaponStatistics.MachineGunTotalSplats += matchData.PlayerStatsTotal.MachineGunKills;
			statistics.WeaponStatistics.MachineGunTotalShotsFired += matchData.PlayerStatsTotal.MachineGunShotsFired;
			statistics.WeaponStatistics.MachineGunTotalShotsHit += matchData.PlayerStatsTotal.MachineGunShotsHit;

			// Shotgun
			statistics.WeaponStatistics.ShotgunTotalDamageDone += matchData.PlayerStatsTotal.ShotgunDamageDone;
			statistics.WeaponStatistics.ShotgunTotalSplats += matchData.PlayerStatsTotal.ShotgunSplats;
			statistics.WeaponStatistics.ShotgunTotalShotsFired += matchData.PlayerStatsTotal.ShotgunShotsFired;
			statistics.WeaponStatistics.ShotgunTotalShotsHit += matchData.PlayerStatsTotal.ShotgunShotsHit;

			// Splattergun
			statistics.WeaponStatistics.SplattergunTotalDamageDone += matchData.PlayerStatsTotal.SplattergunDamageDone;
			statistics.WeaponStatistics.SplattergunTotalSplats += matchData.PlayerStatsTotal.SplattergunKills;
			statistics.WeaponStatistics.SplattergunTotalShotsFired += matchData.PlayerStatsTotal.SplattergunShotsFired;
			statistics.WeaponStatistics.SplattergunTotalShotsHit += matchData.PlayerStatsTotal.SplattergunShotsHit;

			// Sniper Rifle
			statistics.WeaponStatistics.SniperTotalDamageDone += matchData.PlayerStatsTotal.SniperDamageDone;
			statistics.WeaponStatistics.SniperTotalSplats += matchData.PlayerStatsTotal.SniperKills;
			statistics.WeaponStatistics.SniperTotalShotsFired += matchData.PlayerStatsTotal.SniperShotsFired;
			statistics.WeaponStatistics.SniperTotalShotsHit += matchData.PlayerStatsTotal.SniperShotsHit;

			// Melee Weapons
			statistics.WeaponStatistics.MeleeTotalDamageDone += matchData.PlayerStatsTotal.MeleeDamageDone;
			statistics.WeaponStatistics.MeleeTotalSplats += matchData.PlayerStatsTotal.MeleeKills;
			statistics.WeaponStatistics.MeleeTotalShotsFired += matchData.PlayerStatsTotal.MeleeShotsFired;
			statistics.WeaponStatistics.MeleeTotalShotsHit += matchData.PlayerStatsTotal.MeleeShotsHit;

			// Cannon
			statistics.WeaponStatistics.CannonTotalDamageDone += matchData.PlayerStatsTotal.CannonDamageDone;
			statistics.WeaponStatistics.CannonTotalSplats += matchData.PlayerStatsTotal.CannonKills;
			statistics.WeaponStatistics.CannonTotalShotsFired += matchData.PlayerStatsTotal.CannonShotsFired;
			statistics.WeaponStatistics.CannonTotalShotsHit += matchData.PlayerStatsTotal.CannonShotsHit;

			// Launcher
			statistics.WeaponStatistics.LauncherTotalDamageDone += matchData.PlayerStatsTotal.LauncherDamageDone;
			statistics.WeaponStatistics.LauncherTotalSplats += matchData.PlayerStatsTotal.LauncherKills;
			statistics.WeaponStatistics.LauncherTotalShotsFired += matchData.PlayerStatsTotal.LauncherShotsFired;
			statistics.WeaponStatistics.LauncherTotalShotsHit += matchData.PlayerStatsTotal.LauncherShotsHit;

			statistics.PersonalRecord.MostArmorPickedUp = Math.Max(statistics.PersonalRecord.MostArmorPickedUp, bestPerLifeStatistics.ArmorPickedUp);
			statistics.PersonalRecord.MostCannonSplats = Math.Max(statistics.PersonalRecord.MostCannonSplats, bestPerLifeStatistics.CannonKills);
			statistics.PersonalRecord.MostConsecutiveSnipes = Math.Max(statistics.PersonalRecord.MostConsecutiveSnipes, bestPerLifeStatistics.ConsecutiveSnipes);
			statistics.PersonalRecord.MostDamageDealt = Math.Max(statistics.PersonalRecord.MostDamageDealt, bestPerLifeStatistics.GetDamageDealt());
			statistics.PersonalRecord.MostDamageReceived = Math.Max(statistics.PersonalRecord.MostDamageReceived, bestPerLifeStatistics.DamageReceived);
			statistics.PersonalRecord.MostHeadshots = Math.Max(statistics.PersonalRecord.MostHeadshots, bestPerLifeStatistics.Headshots);
			statistics.PersonalRecord.MostHealthPickedUp = Math.Max(statistics.PersonalRecord.MostHealthPickedUp, bestPerLifeStatistics.HealthPickedUp);
			statistics.PersonalRecord.MostLauncherSplats = Math.Max(statistics.PersonalRecord.MostLauncherSplats, bestPerLifeStatistics.LauncherKills);
			statistics.PersonalRecord.MostMachinegunSplats = Math.Max(statistics.PersonalRecord.MostMachinegunSplats, bestPerLifeStatistics.MachineGunKills);
			statistics.PersonalRecord.MostMeleeSplats = Math.Max(statistics.PersonalRecord.MostMeleeSplats, bestPerLifeStatistics.MeleeKills);
			statistics.PersonalRecord.MostNutshots = Math.Max(statistics.PersonalRecord.MostNutshots, bestPerLifeStatistics.Nutshots);
			statistics.PersonalRecord.MostShotgunSplats = Math.Max(statistics.PersonalRecord.MostShotgunSplats, bestPerLifeStatistics.ShotgunSplats);
			statistics.PersonalRecord.MostSniperSplats = Math.Max(statistics.PersonalRecord.MostSniperSplats, bestPerLifeStatistics.SniperKills);
			statistics.PersonalRecord.MostSplats = Math.Max(statistics.PersonalRecord.MostSplats, totalStatistics.GetKills());
			statistics.PersonalRecord.MostSplattergunSplats = Math.Max(statistics.PersonalRecord.MostSplattergunSplats, bestPerLifeStatistics.SplattergunKills);
			statistics.PersonalRecord.MostXPEarned = Math.Max(statistics.PersonalRecord.MostXPEarned, totalStatistics.Xp);

			UserWebServiceClient.Instance.UpdatePlayerStatistics(Peer.AuthToken, statistics);
		}

		public StatsCollection GetBestPerLifeStatistics() {
			StatsCollection result = new StatsCollection {
				Headshots = PerLifeStatistics.Max(_ => _.Headshots),
				Nutshots = PerLifeStatistics.Max(_ => _.Nutshots),
				ConsecutiveSnipes = PerLifeStatistics.Max(_ => _.ConsecutiveSnipes),
				Xp = PerLifeStatistics.Max(_ => _.Xp),
				DamageReceived = PerLifeStatistics.Max(_ => _.DamageReceived),
				ArmorPickedUp = PerLifeStatistics.Max(_ => _.ArmorPickedUp),
				HealthPickedUp = PerLifeStatistics.Max(_ => _.HealthPickedUp),
				MeleeKills = PerLifeStatistics.Max(_ => _.MeleeKills),
				MeleeShotsFired = PerLifeStatistics.Max(_ => _.MeleeShotsFired),
				MeleeShotsHit = PerLifeStatistics.Max(_ => _.MeleeShotsHit),
				MeleeDamageDone = PerLifeStatistics.Max(_ => _.MeleeDamageDone),
				MachineGunKills = PerLifeStatistics.Max(_ => _.MachineGunKills),
				MachineGunShotsFired = PerLifeStatistics.Max(_ => _.MachineGunShotsFired),
				MachineGunShotsHit = PerLifeStatistics.Max(_ => _.MachineGunShotsHit),
				MachineGunDamageDone = PerLifeStatistics.Max(_ => _.MachineGunDamageDone),
				ShotgunSplats = PerLifeStatistics.Max(_ => _.ShotgunSplats),
				ShotgunShotsFired = PerLifeStatistics.Max(_ => _.ShotgunShotsFired),
				ShotgunShotsHit = PerLifeStatistics.Max(_ => _.ShotgunShotsHit),
				ShotgunDamageDone = PerLifeStatistics.Max(_ => _.ShotgunDamageDone),
				SniperKills = PerLifeStatistics.Max(_ => _.SniperKills),
				SniperShotsFired = PerLifeStatistics.Max(_ => _.SniperShotsFired),
				SniperShotsHit = PerLifeStatistics.Max(_ => _.SniperShotsHit),
				SniperDamageDone = PerLifeStatistics.Max(_ => _.SniperDamageDone),
				SplattergunKills = PerLifeStatistics.Max(_ => _.SplattergunKills),
				SplattergunShotsFired = PerLifeStatistics.Max(_ => _.SplattergunShotsFired),
				SplattergunShotsHit = PerLifeStatistics.Max(_ => _.SplattergunShotsHit),
				SplattergunDamageDone = PerLifeStatistics.Max(_ => _.SplattergunDamageDone),
				CannonKills = PerLifeStatistics.Max(_ => _.CannonKills),
				CannonShotsFired = PerLifeStatistics.Max(_ => _.CannonShotsFired),
				CannonShotsHit = PerLifeStatistics.Max(_ => _.CannonShotsHit),
				CannonDamageDone = PerLifeStatistics.Max(_ => _.CannonDamageDone),
				LauncherKills = PerLifeStatistics.Max(_ => _.LauncherKills),
				LauncherShotsFired = PerLifeStatistics.Max(_ => _.LauncherShotsFired),
				LauncherShotsHit = PerLifeStatistics.Max(_ => _.LauncherShotsHit),
				LauncherDamageDone = PerLifeStatistics.Max(_ => _.LauncherDamageDone),


				Deaths = PerLifeStatistics.Max(_ => _.Deaths),
				Suicides = PerLifeStatistics.Max(_ => _.Suicides)
			};

			return result;
		}



		public void IncreaseHeadshots() {
			MatchStatistics.Headshots++;
			CurrentLifeStatistics.Headshots++;
		}

		public void IncreaseNutshots() {
			MatchStatistics.Nutshots++;
			CurrentLifeStatistics.Nutshots++;
		}

		public void IncreaseConsecutiveSnipes() {
			MatchStatistics.ConsecutiveSnipes++;
			CurrentLifeStatistics.ConsecutiveSnipes++;
		}

		public void IncreaseXp(int xp) {
			MatchStatistics.Xp += xp;
			CurrentLifeStatistics.Xp += xp;
		}

		public void IncreaseDeaths() {
			ActorInfo.Deaths++;

			MatchStatistics.ConsecutiveSnipes = 0;
			MatchStatistics.Deaths++;

			PerLifeStatistics.Add(CurrentLifeStatistics);
			ResetCurrentLifeStatistics();
		}

		public void IncreaseDamageReceived(int receivedDamage) {
			MatchStatistics.DamageReceived += receivedDamage;
			CurrentLifeStatistics.DamageReceived += receivedDamage;
		}

		public void IncreaseArmorPickedUp(int armor) {
			MatchStatistics.ArmorPickedUp += armor;
			CurrentLifeStatistics.ArmorPickedUp += armor;
		}

		public void IncreaseHealthPickedUp(int health) {
			MatchStatistics.HealthPickedUp += health;
			CurrentLifeStatistics.HealthPickedUp += health;
		}

		public void IncreaseWeaponKills(UberstrikeItemClass itemClass, BodyPart bodyPart) {
			ActorInfo.Kills++;

			switch (bodyPart) {
				case BodyPart.Head:
					IncreaseHeadshots();
					break;
				case BodyPart.Nuts:
					IncreaseNutshots();
					break;
			}

			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeKills++;
					CurrentLifeStatistics.MeleeKills++;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunKills++;
					CurrentLifeStatistics.MachineGunKills++;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunSplats++;
					CurrentLifeStatistics.ShotgunSplats++;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperKills++;
					CurrentLifeStatistics.SniperKills++;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunKills++;
					CurrentLifeStatistics.SplattergunKills++;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonKills++;
					CurrentLifeStatistics.CannonKills++;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherKills++;
					CurrentLifeStatistics.LauncherKills++;
					break;
				default: break;
			}
		}

		public void IncreaseWeaponShotsFired(UberstrikeItemClass itemClass, int shots) {
			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeShotsFired += shots;
					CurrentLifeStatistics.MeleeShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunShotsFired += shots;
					CurrentLifeStatistics.MachineGunShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunShotsFired += shots;
					CurrentLifeStatistics.ShotgunShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperShotsFired += shots;
					CurrentLifeStatistics.SniperShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunShotsFired += shots;
					CurrentLifeStatistics.SplattergunShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonShotsFired += shots;
					CurrentLifeStatistics.CannonShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherShotsFired += shots;
					CurrentLifeStatistics.LauncherShotsFired += shots;
					break;
				default: break;
			}
		}

		public void IncreaseWeaponShotsHit(UberstrikeItemClass itemClass) {
			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeShotsHit++;
					CurrentLifeStatistics.MeleeShotsHit++;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunShotsHit++;
					CurrentLifeStatistics.MachineGunShotsHit++;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunShotsHit++;
					CurrentLifeStatistics.ShotgunShotsHit++;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperShotsHit++;
					CurrentLifeStatistics.SniperShotsHit++;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunShotsHit++;
					CurrentLifeStatistics.SplattergunShotsHit++;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonShotsHit++;
					CurrentLifeStatistics.CannonShotsHit++;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherShotsHit++;
					CurrentLifeStatistics.LauncherShotsHit++;
					break;
				default: break;
			}
		}

		public void IncreaseWeaponDamageDone(UberstrikeItemClass itemClass, int damage) {
			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeDamageDone += damage;
					CurrentLifeStatistics.MeleeDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunDamageDone += damage;
					CurrentLifeStatistics.MachineGunDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunDamageDone += damage;
					CurrentLifeStatistics.ShotgunDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperDamageDone += damage;
					CurrentLifeStatistics.SniperDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunDamageDone += damage;
					CurrentLifeStatistics.SplattergunDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonDamageDone += damage;
					CurrentLifeStatistics.CannonDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherDamageDone += damage;
					CurrentLifeStatistics.LauncherDamageDone += damage;
					break;
				default: break;
			}
		}

		public void IncreaseSuicides() {
			ActorInfo.Deaths++;

			MatchStatistics.ConsecutiveSnipes = 0;
			MatchStatistics.Suicides++;

			PerLifeStatistics.Add(CurrentLifeStatistics);
			ResetCurrentLifeStatistics();
		}

		public void IncreasePointsEarned(int points) {
			MatchStatistics.Points += points;
		}
	}
}
