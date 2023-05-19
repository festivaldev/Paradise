using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class MysteryBoxWonItemUnityViewProxy {
		public static void Serialize(Stream stream, MysteryBoxWonItemUnityView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.CreditWon);
					Int32Proxy.Serialize(memoryStream, instance.ItemIdWon);
					Int32Proxy.Serialize(memoryStream, instance.PointWon);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static MysteryBoxWonItemUnityView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MysteryBoxWonItemUnityView mysteryBoxWonItemUnityView = null;
			if (num != 0) {
				mysteryBoxWonItemUnityView = new MysteryBoxWonItemUnityView();
				mysteryBoxWonItemUnityView.CreditWon = Int32Proxy.Deserialize(bytes);
				mysteryBoxWonItemUnityView.ItemIdWon = Int32Proxy.Deserialize(bytes);
				mysteryBoxWonItemUnityView.PointWon = Int32Proxy.Deserialize(bytes);
			}
			return mysteryBoxWonItemUnityView;
		}
	}
}
