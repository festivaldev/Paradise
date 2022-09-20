using System;
using System.Collections.Generic;

namespace Paradise.Core.Models.Views {
	[Serializable]
	public class UberStrikeItemShopClientView {
		public List<UberStrikeItemFunctionalView> FunctionalItems { get; set; }

		public List<UberStrikeItemGearView> GearItems { get; set; }

		public List<UberStrikeItemQuickView> QuickItems { get; set; }

		public List<UberStrikeItemWeaponView> WeaponItems { get; set; }

		public Dictionary<int, int> ItemsRecommendationPerMap { get; set; } // # LEGACY # //
	}
}
