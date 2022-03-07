using System;
using System.Collections.Generic;

namespace Paradise.Core.Models {
	[Serializable]
	public class EndOfMatchData {
		public List<StatsSummary> MostValuablePlayers { get; set; }

		public int MostEffecientWeaponId { get; set; }

		public StatsCollection PlayerStatsTotal { get; set; }

		public StatsCollection PlayerStatsBestPerLife { get; set; }

		public Dictionary<byte, ushort> PlayerXpEarned { get; set; }

		public int TimeInGameMinutes { get; set; }

		public bool HasWonMatch { get; set; }

		public string MatchGuid { get; set; }
	}
}
