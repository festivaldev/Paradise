using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MatchStatsProxy {
		public static void Serialize(Stream stream, MatchStats instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				EnumProxy<GameModeType>.Serialize(memoryStream, instance.GameModeId);
				Int32Proxy.Serialize(memoryStream, instance.MapId);
				if (instance.Players != null) {
					ListProxy<PlayerMatchStats>.Serialize(memoryStream, instance.Players, new ListProxy<PlayerMatchStats>.Serializer<PlayerMatchStats>(PlayerMatchStatsProxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.PlayersLimit);
				Int32Proxy.Serialize(memoryStream, instance.TimeLimit);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static MatchStats Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MatchStats matchStats = new MatchStats();
			matchStats.GameModeId = EnumProxy<GameModeType>.Deserialize(bytes);
			matchStats.MapId = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				matchStats.Players = ListProxy<PlayerMatchStats>.Deserialize(bytes, new ListProxy<PlayerMatchStats>.Deserializer<PlayerMatchStats>(PlayerMatchStatsProxy.Deserialize));
			}
			matchStats.PlayersLimit = Int32Proxy.Deserialize(bytes);
			matchStats.TimeLimit = Int32Proxy.Deserialize(bytes);
			return matchStats;
		}
	}
}
