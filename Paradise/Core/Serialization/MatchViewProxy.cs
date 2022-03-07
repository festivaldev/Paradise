using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MatchViewProxy {
		public static void Serialize(Stream stream, MatchView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				EnumProxy<GameModeType>.Serialize(memoryStream, instance.GameModeId);
				Int32Proxy.Serialize(memoryStream, instance.MapId);
				if (instance.PlayersCompleted != null) {
					ListProxy<PlayerStatisticsView>.Serialize(memoryStream, instance.PlayersCompleted, new ListProxy<PlayerStatisticsView>.Serializer<PlayerStatisticsView>(PlayerStatisticsViewProxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.PlayersLimit);
				if (instance.PlayersNonCompleted != null) {
					ListProxy<PlayerStatisticsView>.Serialize(memoryStream, instance.PlayersNonCompleted, new ListProxy<PlayerStatisticsView>.Serializer<PlayerStatisticsView>(PlayerStatisticsViewProxy.Serialize));
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(memoryStream, instance.TimeLimit);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static MatchView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MatchView matchView = new MatchView();
			matchView.GameModeId = EnumProxy<GameModeType>.Deserialize(bytes);
			matchView.MapId = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				matchView.PlayersCompleted = ListProxy<PlayerStatisticsView>.Deserialize(bytes, new ListProxy<PlayerStatisticsView>.Deserializer<PlayerStatisticsView>(PlayerStatisticsViewProxy.Deserialize));
			}
			matchView.PlayersLimit = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				matchView.PlayersNonCompleted = ListProxy<PlayerStatisticsView>.Deserialize(bytes, new ListProxy<PlayerStatisticsView>.Deserializer<PlayerStatisticsView>(PlayerStatisticsViewProxy.Deserialize));
			}
			matchView.TimeLimit = Int32Proxy.Deserialize(bytes);
			return matchView;
		}
	}
}
