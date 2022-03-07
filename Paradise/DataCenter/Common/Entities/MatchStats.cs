using Paradise.Core.Types;
using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class MatchStats {
		public List<PlayerMatchStats> Players { get; set; }

		public int MapId { get; set; }

		public GameModeType GameModeId { get; set; }

		public int TimeLimit { get; set; }

		public int PlayersLimit { get; set; }
	}
}
