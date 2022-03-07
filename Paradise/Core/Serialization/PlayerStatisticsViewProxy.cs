using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PlayerStatisticsViewProxy {
		public static void Serialize(Stream stream, PlayerStatisticsView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.Cmid);
				Int32Proxy.Serialize(memoryStream, instance.Headshots);
				Int64Proxy.Serialize(memoryStream, instance.Hits);
				Int32Proxy.Serialize(memoryStream, instance.Level);
				Int32Proxy.Serialize(memoryStream, instance.Nutshots);
				if (instance.PersonalRecord != null) {
					PlayerPersonalRecordStatisticsViewProxy.Serialize(memoryStream, instance.PersonalRecord);
				} else {
					num |= 1;
				}
				Int64Proxy.Serialize(memoryStream, instance.Shots);
				Int32Proxy.Serialize(memoryStream, instance.Splats);
				Int32Proxy.Serialize(memoryStream, instance.Splatted);
				Int32Proxy.Serialize(memoryStream, instance.TimeSpentInGame);
				if (instance.WeaponStatistics != null) {
					PlayerWeaponStatisticsViewProxy.Serialize(memoryStream, instance.WeaponStatistics);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(memoryStream, instance.Xp);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static PlayerStatisticsView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PlayerStatisticsView playerStatisticsView = new PlayerStatisticsView();
			playerStatisticsView.Cmid = Int32Proxy.Deserialize(bytes);
			playerStatisticsView.Headshots = Int32Proxy.Deserialize(bytes);
			playerStatisticsView.Hits = Int64Proxy.Deserialize(bytes);
			playerStatisticsView.Level = Int32Proxy.Deserialize(bytes);
			playerStatisticsView.Nutshots = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				playerStatisticsView.PersonalRecord = PlayerPersonalRecordStatisticsViewProxy.Deserialize(bytes);
			}
			playerStatisticsView.Shots = Int64Proxy.Deserialize(bytes);
			playerStatisticsView.Splats = Int32Proxy.Deserialize(bytes);
			playerStatisticsView.Splatted = Int32Proxy.Deserialize(bytes);
			playerStatisticsView.TimeSpentInGame = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				playerStatisticsView.WeaponStatistics = PlayerWeaponStatisticsViewProxy.Deserialize(bytes);
			}
			playerStatisticsView.Xp = Int32Proxy.Deserialize(bytes);
			return playerStatisticsView;
		}
	}
}
