using Paradise.Core.Models;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class EndOfMatchDataProxy {
		public static void Serialize(Stream stream, EndOfMatchData instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				BooleanProxy.Serialize(memoryStream, instance.HasWonMatch);
				if (instance.MatchGuid != null) {
					StringProxy.Serialize(memoryStream, instance.MatchGuid);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.MostEffecientWeaponId);
				if (instance.MostValuablePlayers != null) {
					ListProxy<StatsSummary>.Serialize(memoryStream, instance.MostValuablePlayers, new ListProxy<StatsSummary>.Serializer<StatsSummary>(StatsSummaryProxy.Serialize));
				} else {
					num |= 2;
				}
				if (instance.PlayerStatsBestPerLife != null) {
					StatsCollectionProxy.Serialize(memoryStream, instance.PlayerStatsBestPerLife);
				} else {
					num |= 4;
				}
				if (instance.PlayerStatsTotal != null) {
					StatsCollectionProxy.Serialize(memoryStream, instance.PlayerStatsTotal);
				} else {
					num |= 8;
				}
				if (instance.PlayerXpEarned != null) {
					DictionaryProxy<byte, ushort>.Serialize(memoryStream, instance.PlayerXpEarned, new DictionaryProxy<byte, ushort>.Serializer<byte>(ByteProxy.Serialize), new DictionaryProxy<byte, ushort>.Serializer<ushort>(UInt16Proxy.Serialize));
				} else {
					num |= 16;
				}
				Int32Proxy.Serialize(memoryStream, instance.TimeInGameMinutes);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static EndOfMatchData Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			EndOfMatchData endOfMatchData = new EndOfMatchData();
			endOfMatchData.HasWonMatch = BooleanProxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				endOfMatchData.MatchGuid = StringProxy.Deserialize(bytes);
			}
			endOfMatchData.MostEffecientWeaponId = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				endOfMatchData.MostValuablePlayers = ListProxy<StatsSummary>.Deserialize(bytes, new ListProxy<StatsSummary>.Deserializer<StatsSummary>(StatsSummaryProxy.Deserialize));
			}
			if ((num & 4) != 0) {
				endOfMatchData.PlayerStatsBestPerLife = StatsCollectionProxy.Deserialize(bytes);
			}
			if ((num & 8) != 0) {
				endOfMatchData.PlayerStatsTotal = StatsCollectionProxy.Deserialize(bytes);
			}
			if ((num & 16) != 0) {
				endOfMatchData.PlayerXpEarned = DictionaryProxy<byte, ushort>.Deserialize(bytes, new DictionaryProxy<byte, ushort>.Deserializer<byte>(ByteProxy.Deserialize), new DictionaryProxy<byte, ushort>.Deserializer<ushort>(UInt16Proxy.Deserialize));
			}
			endOfMatchData.TimeInGameMinutes = Int32Proxy.Deserialize(bytes);
			return endOfMatchData;
		}
	}
}
