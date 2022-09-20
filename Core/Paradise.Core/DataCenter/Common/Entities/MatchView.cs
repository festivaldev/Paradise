using Paradise.Core.Types;
using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class MatchView {
		public MatchView() {
		}

		public MatchView(List<PlayerStatisticsView> playersCompleted, List<PlayerStatisticsView> playersNonCompleted, int mapId, GameModeType gameModeId, int timeLimit, int playersLimit) {
			this.PlayersCompleted = playersCompleted;
			this.PlayersNonCompleted = playersNonCompleted;
			this.MapId = mapId;
			this.GameModeId = gameModeId;
			this.TimeLimit = timeLimit;
			this.PlayersLimit = playersLimit;
		}

		public List<PlayerStatisticsView> PlayersCompleted { get; set; }

		public List<PlayerStatisticsView> PlayersNonCompleted { get; set; }

		public int MapId { get; set; }

		public GameModeType GameModeId { get; set; }

		public int TimeLimit { get; set; }

		public int PlayersLimit { get; set; }
	}
}
