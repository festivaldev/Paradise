using System;
using System.IO;
using Paradise.DataCenter.Common.Entities;

namespace Paradise.Core.Serialization.Legacy
{
	public static class PlayerPersonalRecordStatisticsViewProxy
	{
		public static void Serialize(Stream stream, PlayerPersonalRecordStatisticsView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.MostArmorPickedUp);
					Int32Proxy.Serialize(memoryStream, instance.MostCannonSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostConsecutiveSnipes);
					Int32Proxy.Serialize(memoryStream, instance.MostDamageDealt);
					Int32Proxy.Serialize(memoryStream, instance.MostDamageReceived);
					Int32Proxy.Serialize(memoryStream, instance.MostHandgunSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostHeadshots);
					Int32Proxy.Serialize(memoryStream, instance.MostHealthPickedUp);
					Int32Proxy.Serialize(memoryStream, instance.MostLauncherSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostMachinegunSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostMeleeSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostNutshots);
					Int32Proxy.Serialize(memoryStream, instance.MostShotgunSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostSniperSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostSplattergunSplats);
					Int32Proxy.Serialize(memoryStream, instance.MostXPEarned);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PlayerPersonalRecordStatisticsView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			PlayerPersonalRecordStatisticsView playerPersonalRecordStatisticsView = null;
			if (num != 0)
			{
				playerPersonalRecordStatisticsView = new PlayerPersonalRecordStatisticsView();
				playerPersonalRecordStatisticsView.MostArmorPickedUp = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostCannonSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostConsecutiveSnipes = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostDamageDealt = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostDamageReceived = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostHandgunSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostHeadshots = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostHealthPickedUp = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostLauncherSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostMachinegunSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostMeleeSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostNutshots = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostShotgunSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostSniperSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostSplattergunSplats = Int32Proxy.Deserialize(bytes);
				playerPersonalRecordStatisticsView.MostXPEarned = Int32Proxy.Deserialize(bytes);
			}
			return playerPersonalRecordStatisticsView;
		}
	}
}
