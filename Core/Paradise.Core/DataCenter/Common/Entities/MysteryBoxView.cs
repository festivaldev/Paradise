using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	public class MysteryBoxView {
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public int Price { get; set; }

		public UberStrikeCurrencyType UberStrikeCurrencyType { get; set; }

		public string IconUrl { get; set; }

		public BundleCategoryType Category { get; set; }

		public bool IsAvailableInShop { get; set; }

		public int ItemsAttributed { get; set; }

		public string ImageUrl { get; set; }

		public bool ExposeItemsToPlayers { get; set; }

		public int PointsAttributed { get; set; }

		public int PointsAttributedWeight { get; set; }

		public int CreditsAttributed { get; set; }

		public int CreditsAttributedWeight { get; set; }

		public List<MysteryBoxItemView> MysteryBoxItems { get; set; }
	}
}
