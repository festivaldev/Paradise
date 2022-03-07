using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MysteryBoxWonItemUnityViewProxy {
		public static void Serialize(Stream stream, MysteryBoxWonItemUnityView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.CreditWon);
				Int32Proxy.Serialize(memoryStream, instance.ItemIdWon);
				Int32Proxy.Serialize(memoryStream, instance.PointWon);
				memoryStream.WriteTo(stream);
			}
		}

		public static MysteryBoxWonItemUnityView Deserialize(Stream bytes) {
			return new MysteryBoxWonItemUnityView {
				CreditWon = Int32Proxy.Deserialize(bytes),
				ItemIdWon = Int32Proxy.Deserialize(bytes),
				PointWon = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
