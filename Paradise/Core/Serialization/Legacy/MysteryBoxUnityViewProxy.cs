using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class MysteryBoxUnityViewProxy
	{
		public static void Serialize(Stream stream, MysteryBoxUnityView instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					EnumProxy<BundleCategoryType>.Serialize(memoryStream, instance.Category);
					Int32Proxy.Serialize(memoryStream, instance.CreditsAttributed);
					Int32Proxy.Serialize(memoryStream, instance.CreditsAttributedWeight);
					if (instance.Description != null)
					{
						StringProxy.Serialize(memoryStream, instance.Description);
					}
					else
					{
						num |= 1;
					}
					BooleanProxy.Serialize(memoryStream, instance.ExposeItemsToPlayers);
					if (instance.IconUrl != null)
					{
						StringProxy.Serialize(memoryStream, instance.IconUrl);
					}
					else
					{
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.Id);
					if (instance.ImageUrl != null)
					{
						StringProxy.Serialize(memoryStream, instance.ImageUrl);
					}
					else
					{
						num |= 4;
					}
					BooleanProxy.Serialize(memoryStream, instance.IsAvailableInShop);
					Int32Proxy.Serialize(memoryStream, instance.ItemsAttributed);
					if (instance.MysteryBoxItems != null)
					{
						ListProxy<BundleItemView>.Serialize(memoryStream, instance.MysteryBoxItems, new ListProxy<BundleItemView>.Serializer<BundleItemView>(BundleItemViewProxy.Serialize));
					}
					else
					{
						num |= 8;
					}
					if (instance.Name != null)
					{
						StringProxy.Serialize(memoryStream, instance.Name);
					}
					else
					{
						num |= 16;
					}
					Int32Proxy.Serialize(memoryStream, instance.PointsAttributed);
					Int32Proxy.Serialize(memoryStream, instance.PointsAttributedWeight);
					Int32Proxy.Serialize(memoryStream, instance.Price);
					EnumProxy<UberStrikeCurrencyType>.Serialize(memoryStream, instance.UberStrikeCurrencyType);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static MysteryBoxUnityView Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			MysteryBoxUnityView mysteryBoxUnityView = null;
			if (num != 0)
			{
				mysteryBoxUnityView = new MysteryBoxUnityView();
				mysteryBoxUnityView.Category = EnumProxy<BundleCategoryType>.Deserialize(bytes);
				mysteryBoxUnityView.CreditsAttributed = Int32Proxy.Deserialize(bytes);
				mysteryBoxUnityView.CreditsAttributedWeight = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0)
				{
					mysteryBoxUnityView.Description = StringProxy.Deserialize(bytes);
				}
				mysteryBoxUnityView.ExposeItemsToPlayers = BooleanProxy.Deserialize(bytes);
				if ((num & 2) != 0)
				{
					mysteryBoxUnityView.IconUrl = StringProxy.Deserialize(bytes);
				}
				mysteryBoxUnityView.Id = Int32Proxy.Deserialize(bytes);
				if ((num & 4) != 0)
				{
					mysteryBoxUnityView.ImageUrl = StringProxy.Deserialize(bytes);
				}
				mysteryBoxUnityView.IsAvailableInShop = BooleanProxy.Deserialize(bytes);
				mysteryBoxUnityView.ItemsAttributed = Int32Proxy.Deserialize(bytes);
				if ((num & 8) != 0)
				{
					mysteryBoxUnityView.MysteryBoxItems = ListProxy<BundleItemView>.Deserialize(bytes, new ListProxy<BundleItemView>.Deserializer<BundleItemView>(BundleItemViewProxy.Deserialize));
				}
				if ((num & 16) != 0)
				{
					mysteryBoxUnityView.Name = StringProxy.Deserialize(bytes);
				}
				mysteryBoxUnityView.PointsAttributed = Int32Proxy.Deserialize(bytes);
				mysteryBoxUnityView.PointsAttributedWeight = Int32Proxy.Deserialize(bytes);
				mysteryBoxUnityView.Price = Int32Proxy.Deserialize(bytes);
				mysteryBoxUnityView.UberStrikeCurrencyType = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
			}
			return mysteryBoxUnityView;
		}
	}
}
