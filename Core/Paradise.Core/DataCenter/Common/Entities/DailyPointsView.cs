using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class DailyPointsView {
		public int Current { get; set; }

		public int PointsTomorrow { get; set; }

		public int PointsMax { get; set; }
	}
}
