﻿using System;
using System.IO;
using Paradise.Core.Models.Views;

namespace Paradise.Core.Serialization.Legacy
{
	public static class UberStrikeItemShopClientViewProxy
	{
		public static void Serialize(Stream stream, UberStrikeItemShopClientView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					if (instance.FunctionalItems != null)
					{
						ListProxy<UberStrikeItemFunctionalView>.Serialize(memoryStream, instance.FunctionalItems, new ListProxy<UberStrikeItemFunctionalView>.Serializer<UberStrikeItemFunctionalView>(UberStrikeItemFunctionalViewProxy.Serialize));
					}
					else
					{
						num |= 1;
					}
					if (instance.GearItems != null)
					{
						ListProxy<UberStrikeItemGearView>.Serialize(memoryStream, instance.GearItems, new ListProxy<UberStrikeItemGearView>.Serializer<UberStrikeItemGearView>(UberStrikeItemGearViewProxy.Serialize));
					}
					else
					{
						num |= 2;
					}
					if (instance.ItemsRecommendationPerMap != null)
					{
						DictionaryProxy<int, int>.Serialize(memoryStream, instance.ItemsRecommendationPerMap, new DictionaryProxy<int, int>.Serializer<int>(Int32Proxy.Serialize), new DictionaryProxy<int, int>.Serializer<int>(Int32Proxy.Serialize));
					}
					else
					{
						num |= 4;
					}
					if (instance.QuickItems != null)
					{
						ListProxy<UberStrikeItemQuickView>.Serialize(memoryStream, instance.QuickItems, new ListProxy<UberStrikeItemQuickView>.Serializer<UberStrikeItemQuickView>(UberStrikeItemQuickViewProxy.Serialize));
					}
					else
					{
						num |= 8;
					}
					if (instance.WeaponItems != null)
					{
						ListProxy<UberStrikeItemWeaponView>.Serialize(memoryStream, instance.WeaponItems, new ListProxy<UberStrikeItemWeaponView>.Serializer<UberStrikeItemWeaponView>(UberStrikeItemWeaponViewProxy.Serialize));
					}
					else
					{
						num |= 16;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static UberStrikeItemShopClientView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			UberStrikeItemShopClientView uberStrikeItemShopClientView = null;
			if (num != 0)
			{
				uberStrikeItemShopClientView = new UberStrikeItemShopClientView();
				if ((num & 1) != 0)
				{
					uberStrikeItemShopClientView.FunctionalItems = ListProxy<UberStrikeItemFunctionalView>.Deserialize(bytes, new ListProxy<UberStrikeItemFunctionalView>.Deserializer<UberStrikeItemFunctionalView>(UberStrikeItemFunctionalViewProxy.Deserialize));
				}
				if ((num & 2) != 0)
				{
					uberStrikeItemShopClientView.GearItems = ListProxy<UberStrikeItemGearView>.Deserialize(bytes, new ListProxy<UberStrikeItemGearView>.Deserializer<UberStrikeItemGearView>(UberStrikeItemGearViewProxy.Deserialize));
				}
				if ((num & 4) != 0)
				{
					uberStrikeItemShopClientView.ItemsRecommendationPerMap = DictionaryProxy<int, int>.Deserialize(bytes, new DictionaryProxy<int, int>.Deserializer<int>(Int32Proxy.Deserialize), new DictionaryProxy<int, int>.Deserializer<int>(Int32Proxy.Deserialize));
				}
				if ((num & 8) != 0)
				{
					uberStrikeItemShopClientView.QuickItems = ListProxy<UberStrikeItemQuickView>.Deserialize(bytes, new ListProxy<UberStrikeItemQuickView>.Deserializer<UberStrikeItemQuickView>(UberStrikeItemQuickViewProxy.Deserialize));
				}
				if ((num & 16) != 0)
				{
					uberStrikeItemShopClientView.WeaponItems = ListProxy<UberStrikeItemWeaponView>.Deserialize(bytes, new ListProxy<UberStrikeItemWeaponView>.Deserializer<UberStrikeItemWeaponView>(UberStrikeItemWeaponViewProxy.Deserialize));
				}
			}
			return uberStrikeItemShopClientView;
		}
	}
}
