using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class CheckApplicationVersionViewProxy {
		public static void Serialize(Stream stream, CheckApplicationVersionView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.ClientVersion != null) {
					ApplicationViewProxy.Serialize(memoryStream, instance.ClientVersion);
				} else {
					num |= 1;
				}
				if (instance.CurrentVersion != null) {
					ApplicationViewProxy.Serialize(memoryStream, instance.CurrentVersion);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static CheckApplicationVersionView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			CheckApplicationVersionView checkApplicationVersionView = new CheckApplicationVersionView();
			if ((num & 1) != 0) {
				checkApplicationVersionView.ClientVersion = ApplicationViewProxy.Deserialize(bytes);
			}
			if ((num & 2) != 0) {
				checkApplicationVersionView.CurrentVersion = ApplicationViewProxy.Deserialize(bytes);
			}
			return checkApplicationVersionView;
		}
	}
}
