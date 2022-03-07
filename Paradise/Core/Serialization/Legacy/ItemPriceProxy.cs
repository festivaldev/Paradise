using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;
using Paradise.Core.Models.Views;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ItemPriceProxy
	{
		public static void Serialize(Stream stream, ItemPrice instance)
		{
			int num = 0;
			if (instance != null)
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					Int32Proxy.Serialize(memoryStream, instance.Amount);
					EnumProxy<UberStrikeCurrencyType>.Serialize(memoryStream, instance.Currency);
					Int32Proxy.Serialize(memoryStream, instance.Discount);
					EnumProxy<BuyingDurationType>.Serialize(memoryStream, instance.Duration);
					EnumProxy<PackType>.Serialize(memoryStream, instance.PackType);
					Int32Proxy.Serialize(memoryStream, instance.Price);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			}
			else
			{
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ItemPrice Deserialize(Stream bytes)
		{
			int num = Int32Proxy.Deserialize(bytes);
			ItemPrice itemPrice = null;
			if (num != 0)
			{
				itemPrice = new ItemPrice();
				itemPrice.Amount = Int32Proxy.Deserialize(bytes);
				itemPrice.Currency = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
				itemPrice.Discount = Int32Proxy.Deserialize(bytes);
				itemPrice.Duration = EnumProxy<BuyingDurationType>.Deserialize(bytes);
				itemPrice.PackType = EnumProxy<PackType>.Deserialize(bytes);
				itemPrice.Price = Int32Proxy.Deserialize(bytes);
			}
			return itemPrice;
		}
	}
}
