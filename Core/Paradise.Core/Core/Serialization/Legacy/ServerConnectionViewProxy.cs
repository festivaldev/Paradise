using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class ServerConnectionViewProxy {
		public static void Serialize(Stream stream, ServerConnectionView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, int.Parse(instance.ApiVersion));
					EnumProxy<ChannelType>.Serialize(memoryStream, instance.Channel);
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static ServerConnectionView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ServerConnectionView serverConnectionView = null;
			if (num != 0) {
				serverConnectionView = new ServerConnectionView();
				serverConnectionView.ApiVersion = Int32Proxy.Deserialize(bytes).ToString();
				serverConnectionView.Channel = EnumProxy<ChannelType>.Deserialize(bytes);
				serverConnectionView.Cmid = Int32Proxy.Deserialize(bytes);
			}
			return serverConnectionView;
		}
	}
}
