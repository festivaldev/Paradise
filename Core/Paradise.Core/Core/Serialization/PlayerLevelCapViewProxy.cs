using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PlayerLevelCapViewProxy {
		public static void Serialize(Stream stream, PlayerLevelCapView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.Level);
				Int32Proxy.Serialize(memoryStream, instance.PlayerLevelCapId);
				Int32Proxy.Serialize(memoryStream, instance.XPRequired);
				memoryStream.WriteTo(stream);
			}
		}

		public static PlayerLevelCapView Deserialize(Stream bytes) {
			return new PlayerLevelCapView {
				Level = Int32Proxy.Deserialize(bytes),
				PlayerLevelCapId = Int32Proxy.Deserialize(bytes),
				XPRequired = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
