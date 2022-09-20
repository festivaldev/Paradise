using System;
using System.IO;
using Paradise.Core.Models.Views;
using Paradise.Core.Types;

namespace Paradise.Core.Serialization.Legacy
{
	public static class UberStrikeItemFunctionalViewProxy
	{
		public static void Serialize(Stream stream, UberStrikeItemFunctionalView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					if (instance.CustomProperties != null)
					{
						DictionaryProxy<string, string>.Serialize(memoryStream, instance.CustomProperties, new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize), new DictionaryProxy<string, string>.Serializer<string>(StringProxy.Serialize));
					}
					else
					{
						num |= 1;
					}
					if (instance.Description != null)
					{
						StringProxy.Serialize(memoryStream, instance.Description);
					}
					else
					{
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.ID);
					BooleanProxy.Serialize(memoryStream, instance.IsConsumable);
					EnumProxy<UberstrikeItemClass>.Serialize(memoryStream, instance.ItemClass);
					Int32Proxy.Serialize(memoryStream, instance.LevelLock);
					if (instance.Name != null)
					{
						StringProxy.Serialize(memoryStream, instance.Name);
					}
					else
					{
						num |= 4;
					}
					if (instance.Prices != null)
					{
						ListProxy<ItemPrice>.Serialize(memoryStream, instance.Prices, new ListProxy<ItemPrice>.Serializer<ItemPrice>(ItemPriceProxy.Serialize));
					}
					else
					{
						num |= 8;
					}
					EnumProxy<ItemShopHighlightType>.Serialize(memoryStream, instance.ShopHighlightType);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static UberStrikeItemFunctionalView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			UberStrikeItemFunctionalView uberStrikeItemFunctionalView = null;
			if (num != 0)
			{
				uberStrikeItemFunctionalView = new UberStrikeItemFunctionalView();
				if ((num & 1) != 0)
				{
					uberStrikeItemFunctionalView.CustomProperties = DictionaryProxy<string, string>.Deserialize(bytes, new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize), new DictionaryProxy<string, string>.Deserializer<string>(StringProxy.Deserialize));
				}
				if ((num & 2) != 0)
				{
					uberStrikeItemFunctionalView.Description = StringProxy.Deserialize(bytes);
				}
				uberStrikeItemFunctionalView.ID = Int32Proxy.Deserialize(bytes);
				uberStrikeItemFunctionalView.IsConsumable = BooleanProxy.Deserialize(bytes);
				uberStrikeItemFunctionalView.ItemClass = EnumProxy<UberstrikeItemClass>.Deserialize(bytes);
				uberStrikeItemFunctionalView.LevelLock = Int32Proxy.Deserialize(bytes);
				if ((num & 4) != 0)
				{
					uberStrikeItemFunctionalView.Name = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0)
				{
					uberStrikeItemFunctionalView.Prices = ListProxy<ItemPrice>.Deserialize(bytes, new ListProxy<ItemPrice>.Deserializer<ItemPrice>(ItemPriceProxy.Deserialize));
				}
				uberStrikeItemFunctionalView.ShopHighlightType = EnumProxy<ItemShopHighlightType>.Deserialize(bytes);
			}
			return uberStrikeItemFunctionalView;
		}
	}
}
