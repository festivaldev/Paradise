using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class LuckyDrawSetUnityView {
		public int Id { get; set; }

		public int SetWeight { get; set; }

		public int CreditsAttributed { get; set; }

		public int PointsAttributed { get; set; }

		public string ImageUrl { get; set; }

		public bool ExposeItemsToPlayers { get; set; }

		public int LuckyDrawId { get; set; }

		public List<BundleItemView> LuckyDrawSetItems { get; set; }
	}
}
