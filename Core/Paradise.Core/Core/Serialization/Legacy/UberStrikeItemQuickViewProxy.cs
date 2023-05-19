using Paradise.Core.Models.Views;
using Paradise.Core.Types;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class UberStrikeItemQuickViewProxy {
		public static void Serialize(Stream stream, UberStrikeItemQuickView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					EnumProxy<QuickItemLogic>.Serialize(memoryStream, instance.BehaviourType);
					Int32Proxy.Serialize(memoryStream, instance.CoolDownTime);
					if (instance.CustomProperties != null) {
						DictionaryProxy<string, string>.Serialize(memoryStream, instance.CustomProperties, new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize), new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize));
					} else {
						num |= 1;
					}
					if (instance.Description != null) {
						StringProxy.Serialize(memoryStream, instance.Description);
					} else {
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.ID);
					BooleanProxy.Serialize(memoryStream, instance.IsConsumable);
					EnumProxy<UberstrikeItemClass>.Serialize(memoryStream, instance.ItemClass);
					Int32Proxy.Serialize(memoryStream, instance.LevelLock);
					Int32Proxy.Serialize(memoryStream, instance.MaxOwnableAmount);
					if (instance.Name != null) {
						StringProxy.Serialize(memoryStream, instance.Name);
					} else {
						num |= 4;
					}
					if (instance.Prices != null) {
						ListProxy<ItemPrice>.Serialize(memoryStream, instance.Prices, new ListProxy<ItemPrice>.Serializer<ItemPrice>(ItemPriceProxy.Serialize));
					} else {
						num |= 8;
					}
					EnumProxy<ItemShopHighlightType>.Serialize(memoryStream, instance.ShopHighlightType);
					Int32Proxy.Serialize(memoryStream, instance.UsesPerGame);
					Int32Proxy.Serialize(memoryStream, instance.UsesPerLife);
					Int32Proxy.Serialize(memoryStream, instance.UsesPerRound);
					Int32Proxy.Serialize(memoryStream, instance.WarmUpTime);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static UberStrikeItemQuickView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			UberStrikeItemQuickView uberStrikeItemQuickView = null;
			if (num != 0) {
				uberStrikeItemQuickView = new UberStrikeItemQuickView();
				uberStrikeItemQuickView.BehaviourType = EnumProxy<QuickItemLogic>.Deserialize(bytes);
				uberStrikeItemQuickView.CoolDownTime = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0) {
					uberStrikeItemQuickView.CustomProperties = DictionaryProxy<string, string>.Deserialize(bytes, new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize), new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize));
				}
				if ((num & 2) != 0) {
					uberStrikeItemQuickView.Description = StringProxy.Deserialize(bytes);
				}
				uberStrikeItemQuickView.ID = Int32Proxy.Deserialize(bytes);
				uberStrikeItemQuickView.IsConsumable = BooleanProxy.Deserialize(bytes);
				uberStrikeItemQuickView.ItemClass = EnumProxy<UberstrikeItemClass>.Deserialize(bytes);
				uberStrikeItemQuickView.LevelLock = Int32Proxy.Deserialize(bytes);
				uberStrikeItemQuickView.MaxOwnableAmount = Int32Proxy.Deserialize(bytes);
				if ((num & 4) != 0) {
					uberStrikeItemQuickView.Name = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0) {
					uberStrikeItemQuickView.Prices = ListProxy<ItemPrice>.Deserialize(bytes, new ListProxy<ItemPrice>.Deserializer<ItemPrice>(ItemPriceProxy.Deserialize));
				}
				uberStrikeItemQuickView.ShopHighlightType = EnumProxy<ItemShopHighlightType>.Deserialize(bytes);
				uberStrikeItemQuickView.UsesPerGame = Int32Proxy.Deserialize(bytes);
				uberStrikeItemQuickView.UsesPerLife = Int32Proxy.Deserialize(bytes);
				uberStrikeItemQuickView.UsesPerRound = Int32Proxy.Deserialize(bytes);
				uberStrikeItemQuickView.WarmUpTime = Int32Proxy.Deserialize(bytes);
			}
			return uberStrikeItemQuickView;
		}
	}
}
