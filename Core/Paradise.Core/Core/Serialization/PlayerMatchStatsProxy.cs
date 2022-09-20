using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PlayerMatchStatsProxy {
		public static void Serialize(Stream stream, PlayerMatchStats instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.Cmid);
				Int32Proxy.Serialize(memoryStream, instance.Death);
				BooleanProxy.Serialize(memoryStream, instance.HasFinishedMatch);
				BooleanProxy.Serialize(memoryStream, instance.HasWonMatch);
				Int32Proxy.Serialize(memoryStream, instance.Headshots);
				Int64Proxy.Serialize(memoryStream, instance.Hits);
				Int32Proxy.Serialize(memoryStream, instance.Kills);
				Int32Proxy.Serialize(memoryStream, instance.Nutshots);
				if (instance.PersonalRecord != null) {
					PlayerPersonalRecordStatisticsViewProxy.Serialize(memoryStream, instance.PersonalRecord);
				} else {
					num |= 1;
				}
				Int64Proxy.Serialize(memoryStream, instance.Shots);
				Int32Proxy.Serialize(memoryStream, instance.Smackdowns);
				Int32Proxy.Serialize(memoryStream, instance.TimeSpentInGame);
				if (instance.WeaponStatistics != null) {
					PlayerWeaponStatisticsViewProxy.Serialize(memoryStream, instance.WeaponStatistics);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static PlayerMatchStats Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PlayerMatchStats playerMatchStats = new PlayerMatchStats();
			playerMatchStats.Cmid = Int32Proxy.Deserialize(bytes);
			playerMatchStats.Death = Int32Proxy.Deserialize(bytes);
			playerMatchStats.HasFinishedMatch = BooleanProxy.Deserialize(bytes);
			playerMatchStats.HasWonMatch = BooleanProxy.Deserialize(bytes);
			playerMatchStats.Headshots = Int32Proxy.Deserialize(bytes);
			playerMatchStats.Hits = Int64Proxy.Deserialize(bytes);
			playerMatchStats.Kills = Int32Proxy.Deserialize(bytes);
			playerMatchStats.Nutshots = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				playerMatchStats.PersonalRecord = PlayerPersonalRecordStatisticsViewProxy.Deserialize(bytes);
			}
			playerMatchStats.Shots = Int64Proxy.Deserialize(bytes);
			playerMatchStats.Smackdowns = Int32Proxy.Deserialize(bytes);
			playerMatchStats.TimeSpentInGame = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				playerMatchStats.WeaponStatistics = PlayerWeaponStatisticsViewProxy.Deserialize(bytes);
			}
			return playerMatchStats;
		}
	}
}
