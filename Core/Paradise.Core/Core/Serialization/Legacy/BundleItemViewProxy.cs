using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class BundleItemViewProxy {
		public static void Serialize(Stream stream, BundleItemView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.Amount);
					Int32Proxy.Serialize(memoryStream, instance.BundleId);
					EnumProxy<BuyingDurationType>.Serialize(memoryStream, instance.Duration);
					Int32Proxy.Serialize(memoryStream, instance.ItemId);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static BundleItemView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			BundleItemView bundleItemView = null;
			if (num != 0) {
				bundleItemView = new BundleItemView();
				bundleItemView.Amount = Int32Proxy.Deserialize(bytes);
				bundleItemView.BundleId = Int32Proxy.Deserialize(bytes);
				bundleItemView.Duration = EnumProxy<BuyingDurationType>.Deserialize(bytes);
				bundleItemView.ItemId = Int32Proxy.Deserialize(bytes);
			}
			return bundleItemView;
		}
	}
}
