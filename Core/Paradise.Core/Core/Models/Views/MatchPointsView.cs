using System;

namespace Paradise.Core.Models.Views {
	[Serializable]
	public class MatchPointsView {
		public int WinnerPointsBase { get; set; }

		public int LoserPointsBase { get; set; }

		public int WinnerPointsPerMinute { get; set; }

		public int LoserPointsPerMinute { get; set; }

		public int MaxTimeInGame { get; set; }
	}
}
