using Paradise.Core.Models.Views;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class UberStrikeItemQuickViewProxy {
		public static void Serialize(Stream stream, UberStrikeItemQuickView instance) {
			int num = 0;
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
				if (instance.ItemProperties != null) {
					DictionaryProxy<ItemPropertyType, int>.Serialize(memoryStream, instance.ItemProperties, new DictionaryProxy<ItemPropertyType, int>.Serializer<ItemPropertyType>(EnumProxy<ItemPropertyType>.Serialize), new DictionaryProxy<ItemPropertyType, int>.Serializer<int>(Int32Proxy.Serialize));
				} else {
					num |= 4;
				}
				Int32Proxy.Serialize(memoryStream, instance.LevelLock);
				Int32Proxy.Serialize(memoryStream, instance.MaxDurationDays);
				Int32Proxy.Serialize(memoryStream, instance.MaxOwnableAmount);
				if (instance.Name != null) {
					StringProxy.Serialize(memoryStream, instance.Name);
				} else {
					num |= 8;
				}
				if (instance.PrefabName != null) {
					StringProxy.Serialize(memoryStream, instance.PrefabName);
				} else {
					num |= 16;
				}
				if (instance.Prices != null) {
					ListProxy<ItemPrice>.Serialize(memoryStream, instance.Prices, new ListProxy<ItemPrice>.Serializer<ItemPrice>(ItemPriceProxy.Serialize));
				} else {
					num |= 32;
				}
				EnumProxy<ItemShopHighlightType>.Serialize(memoryStream, instance.ShopHighlightType);
				Int32Proxy.Serialize(memoryStream, instance.UsesPerGame);
				Int32Proxy.Serialize(memoryStream, instance.UsesPerLife);
				Int32Proxy.Serialize(memoryStream, instance.UsesPerRound);
				Int32Proxy.Serialize(memoryStream, instance.WarmUpTime);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static UberStrikeItemQuickView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			UberStrikeItemQuickView uberStrikeItemQuickView = new UberStrikeItemQuickView();
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
			if ((num & 4) != 0) {
				uberStrikeItemQuickView.ItemProperties = DictionaryProxy<ItemPropertyType, int>.Deserialize(bytes, new DictionaryProxy<ItemPropertyType, int>.Deserializer<ItemPropertyType>(EnumProxy<ItemPropertyType>.Deserialize), new DictionaryProxy<ItemPropertyType, int>.Deserializer<int>(Int32Proxy.Deserialize));
			}
			uberStrikeItemQuickView.LevelLock = Int32Proxy.Deserialize(bytes);
			uberStrikeItemQuickView.MaxDurationDays = Int32Proxy.Deserialize(bytes);
			uberStrikeItemQuickView.MaxOwnableAmount = Int32Proxy.Deserialize(bytes);
			if ((num & 8) != 0) {
				uberStrikeItemQuickView.Name = StringProxy.Deserialize(bytes);
			}
			if ((num & 16) != 0) {
				uberStrikeItemQuickView.PrefabName = StringProxy.Deserialize(bytes);
			}
			if ((num & 32) != 0) {
				uberStrikeItemQuickView.Prices = ListProxy<ItemPrice>.Deserialize(bytes, new ListProxy<ItemPrice>.Deserializer<ItemPrice>(ItemPriceProxy.Deserialize));
			}
			uberStrikeItemQuickView.ShopHighlightType = EnumProxy<ItemShopHighlightType>.Deserialize(bytes);
			uberStrikeItemQuickView.UsesPerGame = Int32Proxy.Deserialize(bytes);
			uberStrikeItemQuickView.UsesPerLife = Int32Proxy.Deserialize(bytes);
			uberStrikeItemQuickView.UsesPerRound = Int32Proxy.Deserialize(bytes);
			uberStrikeItemQuickView.WarmUpTime = Int32Proxy.Deserialize(bytes);
			return uberStrikeItemQuickView;
		}
	}
}
