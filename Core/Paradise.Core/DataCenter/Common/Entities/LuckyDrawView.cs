using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	public class LuckyDrawView {
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public int Price { get; set; }

		public UberStrikeCurrencyType UberStrikeCurrencyType { get; set; }

		public string IconUrl { get; set; }

		public BundleCategoryType Category { get; set; }

		public bool IsAvailableInShop { get; set; }

		public List<LuckyDrawSetView> LuckyDrawSets { get; set; }

		public bool IsEnabled { get; set; }
	}
}
