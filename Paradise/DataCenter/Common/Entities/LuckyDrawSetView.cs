using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	public class LuckyDrawSetView {
		public int Id { get; set; }

		public int SetWeight { get; set; }

		public int CreditsAttributed { get; set; }

		public int PointsAttributed { get; set; }

		public string ImageUrl { get; set; }

		public bool ExposeItemsToPlayers { get; set; }

		public int LuckyDrawId { get; set; }

		public List<LuckyDrawSetItemView> LuckyDrawSetItems { get; set; }
	}
}
