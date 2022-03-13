using Paradise.Core.Models.Views;
using Paradise.WebServices.Client;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class ShopManager {
		public bool IsLoaded { get; private set; }

		public Dictionary<int, UberStrikeItemFunctionalView> FunctionalItems { get; private set; }
		public Dictionary<int, UberStrikeItemGearView> GearItems { get; private set; }
		public Dictionary<int, UberStrikeItemQuickView> QuickItems { get; private set; }
		public Dictionary<int, UberStrikeItemWeaponView> WeaponItems { get; private set; }

		public void Load() {
			var shop = new ShopWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetShop();

			FunctionalItems = LoadDictionary(shop.FunctionalItems);
			GearItems = LoadDictionary(shop.GearItems);
			QuickItems = LoadDictionary(shop.QuickItems);
			WeaponItems = LoadDictionary(shop.WeaponItems);

			IsLoaded = true;
		}

		private Dictionary<int, TItem> LoadDictionary<TItem>(List<TItem> list) where TItem : BaseUberStrikeItemView {
			var dict = new Dictionary<int, TItem>(list.Count);
			foreach (var item in list) {
				dict.Add(item.ID, item);
			}

			return dict;
		}
	}
}
