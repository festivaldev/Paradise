using Paradise.Core.Models.Views;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ItemPriceProxy {
		public static void Serialize(Stream stream, ItemPrice instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.Amount);
				EnumProxy<UberStrikeCurrencyType>.Serialize(memoryStream, instance.Currency);
				Int32Proxy.Serialize(memoryStream, instance.Discount);
				EnumProxy<BuyingDurationType>.Serialize(memoryStream, instance.Duration);
				EnumProxy<PackType>.Serialize(memoryStream, instance.PackType);
				Int32Proxy.Serialize(memoryStream, instance.Price);
				memoryStream.WriteTo(stream);
			}
		}

		public static ItemPrice Deserialize(Stream bytes) {
			return new ItemPrice {
				Amount = Int32Proxy.Deserialize(bytes),
				Currency = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes),
				Discount = Int32Proxy.Deserialize(bytes),
				Duration = EnumProxy<BuyingDurationType>.Deserialize(bytes),
				PackType = EnumProxy<PackType>.Deserialize(bytes),
				Price = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
