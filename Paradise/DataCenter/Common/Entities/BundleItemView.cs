using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class BundleItemView {
		public int BundleId { get; set; }

		public int ItemId { get; set; }

		public int Amount { get; set; }

		public BuyingDurationType Duration { get; set; }
	}
}
