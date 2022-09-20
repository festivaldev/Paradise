using Paradise.Core.Models.Views;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MapSettingsProxy {
		public static void Serialize(Stream stream, MapSettings instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.KillsCurrent);
				Int32Proxy.Serialize(memoryStream, instance.KillsMax);
				Int32Proxy.Serialize(memoryStream, instance.KillsMin);
				Int32Proxy.Serialize(memoryStream, instance.PlayersCurrent);
				Int32Proxy.Serialize(memoryStream, instance.PlayersMax);
				Int32Proxy.Serialize(memoryStream, instance.PlayersMin);
				Int32Proxy.Serialize(memoryStream, instance.TimeCurrent);
				Int32Proxy.Serialize(memoryStream, instance.TimeMax);
				Int32Proxy.Serialize(memoryStream, instance.TimeMin);
				memoryStream.WriteTo(stream);
			}
		}

		public static MapSettings Deserialize(Stream bytes) {
			return new MapSettings {
				KillsCurrent = Int32Proxy.Deserialize(bytes),
				KillsMax = Int32Proxy.Deserialize(bytes),
				KillsMin = Int32Proxy.Deserialize(bytes),
				PlayersCurrent = Int32Proxy.Deserialize(bytes),
				PlayersMax = Int32Proxy.Deserialize(bytes),
				PlayersMin = Int32Proxy.Deserialize(bytes),
				TimeCurrent = Int32Proxy.Deserialize(bytes),
				TimeMax = Int32Proxy.Deserialize(bytes),
				TimeMin = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
