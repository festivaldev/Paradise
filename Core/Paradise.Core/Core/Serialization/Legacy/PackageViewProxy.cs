using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class PackageViewProxy {
		public static void Serialize(Stream stream, PackageView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.Bonus);
					if (instance.Items != null) {
						ListProxy<int>.Serialize(memoryStream, instance.Items, new ListProxy<int>.Serializer<int>(Int32Proxy.Serialize));
					} else {
						num |= 1;
					}
					if (instance.Name != null) {
						StringProxy.Serialize(memoryStream, instance.Name);
					} else {
						num |= 2;
					}
					DecimalProxy.Serialize(memoryStream, instance.Price);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PackageView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PackageView packageView = null;
			if (num != 0) {
				packageView = new PackageView();
				packageView.Bonus = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0) {
					packageView.Items = ListProxy<int>.Deserialize(bytes, new ListProxy<int>.Deserializer<int>(Int32Proxy.Deserialize));
				}
				if ((num & 2) != 0) {
					packageView.Name = StringProxy.Deserialize(bytes);
				}
				packageView.Price = DecimalProxy.Deserialize(bytes);
			}
			return packageView;
		}
	}
}
