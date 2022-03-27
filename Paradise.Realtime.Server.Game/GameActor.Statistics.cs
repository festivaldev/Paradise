using Paradise.Core.Models;
using Paradise.Core.Types;
using System;

namespace Paradise.Realtime.Server.Game {
	public partial class GameActor {
		public StatsCollection MatchStatistics { get; private set; } = new StatsCollection();

		public short Kills => (short)MatchStatistics.GetKills();
		public short Deaths => (short)MatchStatistics.Deaths;
		public double KillDeathRatio => Kills / Math.Max(1, (int)Deaths);
		public double Accuracy => (MatchStatistics.GetHits() / Math.Max(1, MatchStatistics.GetShots())) * 100;

		public void ResetStatistics() {
			MatchStatistics = new StatsCollection();
		}

		public void IncreaseHeadshots() {
			MatchStatistics.Headshots++;
		}

		public void IncreaseNutshots() {
			MatchStatistics.Nutshots++;
		}

		public void IncreaseConsecutiveSnipes() {
			MatchStatistics.ConsecutiveSnipes++;
		}

		public void IncreaseXp(int xp) {
			MatchStatistics.Xp += xp;
		}

		public void IncreaseDeaths() {
			MatchStatistics.ConsecutiveSnipes = 0;
			MatchStatistics.Deaths++;
		}

		public void IncreaseDamageReceived(int receivedDamage) {
			MatchStatistics.DamageReceived += receivedDamage;
		}

		public void IncreaseArmorPickedUp(int armor) {
			MatchStatistics.ArmorPickedUp += armor;
		}

		public void IncreaseHealthPickedUp(int health) {
			MatchStatistics.HealthPickedUp += health;
		}

		public void IncreaseWeaponKills(UberstrikeItemClass itemClass, BodyPart bodyPart) {
			switch (bodyPart) {
				case BodyPart.Head:
					MatchStatistics.Headshots++;
					break;
				case BodyPart.Nuts:
					MatchStatistics.Nutshots++;
					break;
			}

			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeKills++;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunKills++;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunSplats++;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperKills++;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunKills++;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonKills++;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherKills++;
					break;
				default: break;
			}
		}

		public void IncreaseWeaponShotsFired(UberstrikeItemClass itemClass, int shots) {
			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonShotsFired += shots;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherShotsFired += shots;
					break;
				default: break;
			}
		}

		public void IncreaseWeaponShotsHit(UberstrikeItemClass itemClass) {
			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeShotsHit++;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunShotsHit++;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunShotsHit++;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperShotsHit++;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunShotsHit++;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonShotsHit++;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherShotsHit++;
					break;
				default: break;
			}
		}

		public void IncreaseWeaponDamageDone(UberstrikeItemClass itemClass, int damage) {
			switch (itemClass) {
				case UberstrikeItemClass.WeaponMelee:
					MatchStatistics.MeleeDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponMachinegun:
					MatchStatistics.MachineGunDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponShotgun:
					MatchStatistics.ShotgunDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponSniperRifle:
					MatchStatistics.SniperDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponSplattergun:
					MatchStatistics.SplattergunDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponCannon:
					MatchStatistics.CannonDamageDone += damage;
					break;
				case UberstrikeItemClass.WeaponLauncher:
					MatchStatistics.LauncherDamageDone += damage;
					break;
				default: break;
			}
		}

		public void IncreaseSuicides() {
			MatchStatistics.Suicides++;
		}

		public void IncreasePointsEarned(int points) {
			MatchStatistics.Points += points;
		}
	}
}
