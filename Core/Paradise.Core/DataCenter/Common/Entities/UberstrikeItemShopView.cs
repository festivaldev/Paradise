using System.Collections.Generic;
using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	public class UberstrikeItemShopView {
		public UberstrikeItemShopView() {
			this.FunctionalItems = new List<UberstrikeItemFunctionalView>();
			this.GearItems = new List<UberstrikeItemGearView>();
			this.QuickUseItems = new List<UberstrikeItemQuickUseView>();
			this.SpecialItems = new List<UberstrikeItemSpecialView>();
			this.WeaponItems = new List<UberstrikeItemWeaponView>();
			this.WeaponModItems = new List<UberstrikeItemWeaponModView>();
		}

		public UberstrikeItemShopView(List<UberstrikeItemFunctionalView> functionalItems, List<UberstrikeItemGearView> gearItems, List<UberstrikeItemQuickUseView> quickUseItems, List<UberstrikeItemSpecialView> specialItems, List<UberstrikeItemWeaponView> weaponItems, List<UberstrikeItemWeaponModView> weaponModItems, int discoutPointsSevenDays, int discountPointsThirtyDays, int discountPointsNinetyDays, int discountCreditsSevenDays, int discountCreditsThirtyDays, int discountCreditsNinetyDays) {
			this.FunctionalItems = functionalItems;
			this.GearItems = gearItems;
			this.QuickUseItems = quickUseItems;
			this.SpecialItems = specialItems;
			this.WeaponItems = weaponItems;
			this.WeaponModItems = weaponModItems;
			this.DiscountPointsSevenDays = discoutPointsSevenDays;
			this.DiscountPointsThirtyDays = discountPointsThirtyDays;
			this.DiscountPointsNinetyDays = discountPointsNinetyDays;
			this.DiscountCreditsSevenDays = discountCreditsSevenDays;
			this.DiscountCreditsThirtyDays = discountCreditsThirtyDays;
			this.DiscountCreditsNinetyDays = discountCreditsNinetyDays;
		}

		public List<UberstrikeItemFunctionalView> FunctionalItems { get; set; }

		public List<UberstrikeItemGearView> GearItems { get; set; }

		public List<UberstrikeItemQuickUseView> QuickUseItems { get; set; }

		public List<UberstrikeItemSpecialView> SpecialItems { get; set; }

		public List<UberstrikeItemWeaponModView> WeaponModItems { get; set; }

		public List<UberstrikeItemWeaponView> WeaponItems { get; set; }

		public int DiscountPointsSevenDays { get; set; }

		public int DiscountPointsThirtyDays { get; set; }

		public int DiscountPointsNinetyDays { get; set; }

		public int DiscountCreditsSevenDays { get; set; }

		public int DiscountCreditsThirtyDays { get; set; }

		public int DiscountCreditsNinetyDays { get; set; }

		public Dictionary<int, int> ItemsRecommendationPerMap { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[UberstrikeItemShopView: ");
			stringBuilder.Append("[FunctionalItems: ");
			if (this.FunctionalItems != null && this.FunctionalItems.Count > 0) {
				foreach (UberstrikeItemFunctionalView value in this.FunctionalItems) {
					stringBuilder.Append(value);
				}
			}
			stringBuilder.Append("][GearItems: ");
			if (this.GearItems != null && this.GearItems.Count > 0) {
				foreach (UberstrikeItemGearView value2 in this.GearItems) {
					stringBuilder.Append(value2);
				}
			}
			stringBuilder.Append("][QuickUseItems: ");
			if (this.QuickUseItems != null && this.QuickUseItems.Count > 0) {
				foreach (UberstrikeItemQuickUseView value3 in this.QuickUseItems) {
					stringBuilder.Append(value3);
				}
			}
			stringBuilder.Append("][SpecialItems: ");
			if (this.SpecialItems != null && this.SpecialItems.Count > 0) {
				foreach (UberstrikeItemSpecialView value4 in this.SpecialItems) {
					stringBuilder.Append(value4);
				}
			}
			stringBuilder.Append("][WeaponItems: ");
			if (this.WeaponItems != null && this.WeaponItems.Count > 0) {
				foreach (UberstrikeItemWeaponView value5 in this.WeaponItems) {
					stringBuilder.Append(value5);
				}
			}
			stringBuilder.Append("][WeaponModItems: ");
			if (this.WeaponModItems != null && this.WeaponModItems.Count > 0) {
				foreach (UberstrikeItemWeaponModView value6 in this.WeaponModItems) {
					stringBuilder.Append(value6);
				}
			}
			stringBuilder.Append("[DiscountPointsSevenDays: ");
			stringBuilder.Append(this.DiscountPointsSevenDays);
			stringBuilder.Append("%][DiscountPointsThirtyDays: ");
			stringBuilder.Append(this.DiscountPointsThirtyDays);
			stringBuilder.Append("%][DiscountPointsNinetyDays: ");
			stringBuilder.Append(this.DiscountPointsNinetyDays);
			stringBuilder.Append("%][DiscountCreditsSevenDays: ");
			stringBuilder.Append(this.DiscountCreditsSevenDays);
			stringBuilder.Append("%][DiscountCreditsThirtyDays: ");
			stringBuilder.Append(this.DiscountCreditsThirtyDays);
			stringBuilder.Append("%][DiscountCreditsNinetyDays: ");
			stringBuilder.Append(this.DiscountCreditsNinetyDays);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
