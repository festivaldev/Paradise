using Paradise.Core.Models.Views;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MatchPointsViewProxy {
		public static void Serialize(Stream stream, MatchPointsView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.LoserPointsBase);
				Int32Proxy.Serialize(memoryStream, instance.LoserPointsPerMinute);
				Int32Proxy.Serialize(memoryStream, instance.MaxTimeInGame);
				Int32Proxy.Serialize(memoryStream, instance.WinnerPointsBase);
				Int32Proxy.Serialize(memoryStream, instance.WinnerPointsPerMinute);
				memoryStream.WriteTo(stream);
			}
		}

		public static MatchPointsView Deserialize(Stream bytes) {
			return new MatchPointsView {
				LoserPointsBase = Int32Proxy.Deserialize(bytes),
				LoserPointsPerMinute = Int32Proxy.Deserialize(bytes),
				MaxTimeInGame = Int32Proxy.Deserialize(bytes),
				WinnerPointsBase = Int32Proxy.Deserialize(bytes),
				WinnerPointsPerMinute = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
