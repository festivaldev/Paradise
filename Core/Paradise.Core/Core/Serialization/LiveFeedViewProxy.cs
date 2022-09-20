using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class LiveFeedViewProxy {
		public static void Serialize(Stream stream, LiveFeedView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				DateTimeProxy.Serialize(memoryStream, instance.Date);
				if (instance.Description != null) {
					StringProxy.Serialize(memoryStream, instance.Description);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.LivedFeedId);
				Int32Proxy.Serialize(memoryStream, instance.Priority);
				if (instance.Url != null) {
					StringProxy.Serialize(memoryStream, instance.Url);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static LiveFeedView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			LiveFeedView liveFeedView = new LiveFeedView();
			liveFeedView.Date = DateTimeProxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				liveFeedView.Description = StringProxy.Deserialize(bytes);
			}
			liveFeedView.LivedFeedId = Int32Proxy.Deserialize(bytes);
			liveFeedView.Priority = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				liveFeedView.Url = StringProxy.Deserialize(bytes);
			}
			return liveFeedView;
		}
	}
}
