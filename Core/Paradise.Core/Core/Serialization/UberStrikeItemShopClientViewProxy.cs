using Paradise.Core.Models.Views;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class UberStrikeItemShopClientViewProxy {
		public static void Serialize(Stream stream, UberStrikeItemShopClientView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.FunctionalItems != null) {
					ListProxy<UberStrikeItemFunctionalView>.Serialize(memoryStream, instance.FunctionalItems, new ListProxy<UberStrikeItemFunctionalView>.Serializer<UberStrikeItemFunctionalView>(UberStrikeItemFunctionalViewProxy.Serialize));
				} else {
					num |= 1;
				}
				if (instance.GearItems != null) {
					ListProxy<UberStrikeItemGearView>.Serialize(memoryStream, instance.GearItems, new ListProxy<UberStrikeItemGearView>.Serializer<UberStrikeItemGearView>(UberStrikeItemGearViewProxy.Serialize));
				} else {
					num |= 2;
				}
				if (instance.QuickItems != null) {
					ListProxy<UberStrikeItemQuickView>.Serialize(memoryStream, instance.QuickItems, new ListProxy<UberStrikeItemQuickView>.Serializer<UberStrikeItemQuickView>(UberStrikeItemQuickViewProxy.Serialize));
				} else {
					num |= 4;
				}
				if (instance.WeaponItems != null) {
					ListProxy<UberStrikeItemWeaponView>.Serialize(memoryStream, instance.WeaponItems, new ListProxy<UberStrikeItemWeaponView>.Serializer<UberStrikeItemWeaponView>(UberStrikeItemWeaponViewProxy.Serialize));
				} else {
					num |= 8;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static UberStrikeItemShopClientView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			UberStrikeItemShopClientView uberStrikeItemShopClientView = new UberStrikeItemShopClientView();
			if ((num & 1) != 0) {
				uberStrikeItemShopClientView.FunctionalItems = ListProxy<UberStrikeItemFunctionalView>.Deserialize(bytes, new ListProxy<UberStrikeItemFunctionalView>.Deserializer<UberStrikeItemFunctionalView>(UberStrikeItemFunctionalViewProxy.Deserialize));
			}
			if ((num & 2) != 0) {
				uberStrikeItemShopClientView.GearItems = ListProxy<UberStrikeItemGearView>.Deserialize(bytes, new ListProxy<UberStrikeItemGearView>.Deserializer<UberStrikeItemGearView>(UberStrikeItemGearViewProxy.Deserialize));
			}
			if ((num & 4) != 0) {
				uberStrikeItemShopClientView.QuickItems = ListProxy<UberStrikeItemQuickView>.Deserialize(bytes, new ListProxy<UberStrikeItemQuickView>.Deserializer<UberStrikeItemQuickView>(UberStrikeItemQuickViewProxy.Deserialize));
			}
			if ((num & 8) != 0) {
				uberStrikeItemShopClientView.WeaponItems = ListProxy<UberStrikeItemWeaponView>.Deserialize(bytes, new ListProxy<UberStrikeItemWeaponView>.Deserializer<UberStrikeItemWeaponView>(UberStrikeItemWeaponViewProxy.Deserialize));
			}
			return uberStrikeItemShopClientView;
		}
	}
}
