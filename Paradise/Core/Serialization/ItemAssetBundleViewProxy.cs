using Paradise.Core.Models.Views;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ItemAssetBundleViewProxy {
		public static void Serialize(Stream stream, ItemAssetBundleView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.Url != null) {
					StringProxy.Serialize(memoryStream, instance.Url);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static ItemAssetBundleView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ItemAssetBundleView itemAssetBundleView = new ItemAssetBundleView();
			if ((num & 1) != 0) {
				itemAssetBundleView.Url = StringProxy.Deserialize(bytes);
			}
			return itemAssetBundleView;
		}
	}
}
