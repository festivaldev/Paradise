using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ItemTransactionViewProxy {
		public static void Serialize(Stream stream, ItemTransactionView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.Cmid);
				Int32Proxy.Serialize(memoryStream, instance.Credits);
				EnumProxy<BuyingDurationType>.Serialize(memoryStream, instance.Duration);
				BooleanProxy.Serialize(memoryStream, instance.IsAdminAction);
				Int32Proxy.Serialize(memoryStream, instance.ItemId);
				Int32Proxy.Serialize(memoryStream, instance.Points);
				DateTimeProxy.Serialize(memoryStream, instance.WithdrawalDate);
				Int32Proxy.Serialize(memoryStream, instance.WithdrawalId);
				memoryStream.WriteTo(stream);
			}
		}

		public static ItemTransactionView Deserialize(Stream bytes) {
			return new ItemTransactionView {
				Cmid = Int32Proxy.Deserialize(bytes),
				Credits = Int32Proxy.Deserialize(bytes),
				Duration = EnumProxy<BuyingDurationType>.Deserialize(bytes),
				IsAdminAction = BooleanProxy.Deserialize(bytes),
				ItemId = Int32Proxy.Deserialize(bytes),
				Points = Int32Proxy.Deserialize(bytes),
				WithdrawalDate = DateTimeProxy.Deserialize(bytes),
				WithdrawalId = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
