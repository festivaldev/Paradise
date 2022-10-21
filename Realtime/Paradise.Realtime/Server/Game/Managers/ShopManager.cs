using log4net;
using Paradise.Core.Models.Views;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class ShopManager {
		private static readonly ILog Log = LogManager.GetLogger("GameLog");

		public bool IsLoaded { get; private set; }

		public Dictionary<int, UberStrikeItemFunctionalView> FunctionalItems { get; set; }
		public Dictionary<int, UberStrikeItemGearView> GearItems { get; set; }
		public Dictionary<int, UberStrikeItemQuickView> QuickItems { get; set; }
		public Dictionary<int, UberStrikeItemWeaponView> WeaponItems { get; set; }

		public void Load() {
			var shopData = ShopWebServiceClient.Instance.GetShop();

			FunctionalItems = LoadShopItems(shopData.FunctionalItems);
            GearItems = LoadShopItems(shopData.GearItems);
            QuickItems = LoadShopItems(shopData.QuickItems);
            WeaponItems = LoadShopItems(shopData.WeaponItems);

            IsLoaded = true;
		}

		private Dictionary<int, T> LoadShopItems<T>(List<T> items) where T : BaseUberStrikeItemView {
			var dict = new Dictionary<int, T>(items.Count);
			foreach (var item in items) {
				dict.Add(item.ID, item);
			}

			return dict;
		}
	}
}
