using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PlayerMatchStats {
		public int Cmid { get; set; }

		public int Kills { get; set; }

		public int Death { get; set; }

		public long Shots { get; set; }

		public long Hits { get; set; }

		public int TimeSpentInGame { get; set; }

		public int Headshots { get; set; }

		public int Nutshots { get; set; }

		public int Smackdowns { get; set; }

		public bool HasFinishedMatch { get; set; }

		public bool HasWonMatch { get; set; }

		public PlayerPersonalRecordStatisticsView PersonalRecord { get; set; }

		public PlayerWeaponStatisticsView WeaponStatistics { get; set; }
	}
}
