using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class BugViewProxy {
		public static void Serialize(Stream stream, BugView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.Content != null) {
					StringProxy.Serialize(memoryStream, instance.Content);
				} else {
					num |= 1;
				}
				if (instance.Subject != null) {
					StringProxy.Serialize(memoryStream, instance.Subject);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static BugView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			BugView bugView = new BugView();
			if ((num & 1) != 0) {
				bugView.Content = StringProxy.Deserialize(bytes);
			}
			if ((num & 2) != 0) {
				bugView.Subject = StringProxy.Deserialize(bytes);
			}
			return bugView;
		}
	}
}
