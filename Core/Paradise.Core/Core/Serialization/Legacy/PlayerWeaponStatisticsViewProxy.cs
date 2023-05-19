using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class PlayerWeaponStatisticsViewProxy {
		public static void Serialize(Stream stream, PlayerWeaponStatisticsView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.CannonTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.CannonTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.CannonTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.CannonTotalSplats);
					Int32Proxy.Serialize(memoryStream, instance.HandgunTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.HandgunTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.HandgunTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.HandgunTotalSplats);
					Int32Proxy.Serialize(memoryStream, instance.LauncherTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.LauncherTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.LauncherTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.LauncherTotalSplats);
					Int32Proxy.Serialize(memoryStream, instance.MachineGunTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.MachineGunTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.MachineGunTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.MachineGunTotalSplats);
					Int32Proxy.Serialize(memoryStream, instance.MeleeTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.MeleeTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.MeleeTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.MeleeTotalSplats);
					Int32Proxy.Serialize(memoryStream, instance.ShotgunTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.ShotgunTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.ShotgunTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.ShotgunTotalSplats);
					Int32Proxy.Serialize(memoryStream, instance.SniperTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.SniperTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.SniperTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.SniperTotalSplats);
					Int32Proxy.Serialize(memoryStream, instance.SplattergunTotalDamageDone);
					Int32Proxy.Serialize(memoryStream, instance.SplattergunTotalShotsFired);
					Int32Proxy.Serialize(memoryStream, instance.SplattergunTotalShotsHit);
					Int32Proxy.Serialize(memoryStream, instance.SplattergunTotalSplats);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PlayerWeaponStatisticsView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PlayerWeaponStatisticsView playerWeaponStatisticsView = null;
			if (num != 0) {
				playerWeaponStatisticsView = new PlayerWeaponStatisticsView();
				playerWeaponStatisticsView.CannonTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.CannonTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.CannonTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.CannonTotalSplats = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.HandgunTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.HandgunTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.HandgunTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.HandgunTotalSplats = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.LauncherTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.LauncherTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.LauncherTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.LauncherTotalSplats = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MachineGunTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MachineGunTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MachineGunTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MachineGunTotalSplats = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MeleeTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MeleeTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MeleeTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.MeleeTotalSplats = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.ShotgunTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.ShotgunTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.ShotgunTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.ShotgunTotalSplats = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SniperTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SniperTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SniperTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SniperTotalSplats = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SplattergunTotalDamageDone = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SplattergunTotalShotsFired = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SplattergunTotalShotsHit = Int32Proxy.Deserialize(bytes);
				playerWeaponStatisticsView.SplattergunTotalSplats = Int32Proxy.Deserialize(bytes);
			}
			return playerWeaponStatisticsView;
		}
	}
}
