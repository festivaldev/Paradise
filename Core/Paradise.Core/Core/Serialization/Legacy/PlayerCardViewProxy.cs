using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class PlayerCardViewProxy {
		public static void Serialize(Stream stream, PlayerCardView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					Int64Proxy.Serialize(memoryStream, instance.Hits);
					if (instance.Name != null) {
						StringProxy.Serialize(memoryStream, instance.Name);
					} else {
						num |= 1;
					}
					if (instance.Precision != null) {
						StringProxy.Serialize(memoryStream, instance.Precision);
					} else {
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.Ranking);
					Int64Proxy.Serialize(memoryStream, instance.Shots);
					Int32Proxy.Serialize(memoryStream, instance.Splats);
					Int32Proxy.Serialize(memoryStream, instance.Splatted);
					if (instance.TagName != null) {
						StringProxy.Serialize(memoryStream, instance.TagName);
					} else {
						num |= 4;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PlayerCardView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PlayerCardView playerCardView = null;
			if (num != 0) {
				playerCardView = new PlayerCardView();
				playerCardView.Cmid = Int32Proxy.Deserialize(bytes);
				playerCardView.Hits = Int64Proxy.Deserialize(bytes);
				if ((num & 1) != 0) {
					playerCardView.Name = StringProxy.Deserialize(bytes);
				}
				if ((num & 2) != 0) {
					playerCardView.Precision = StringProxy.Deserialize(bytes);
				}
				playerCardView.Ranking = Int32Proxy.Deserialize(bytes);
				playerCardView.Shots = Int64Proxy.Deserialize(bytes);
				playerCardView.Splats = Int32Proxy.Deserialize(bytes);
				playerCardView.Splatted = Int32Proxy.Deserialize(bytes);
				if ((num & 4) != 0) {
					playerCardView.TagName = StringProxy.Deserialize(bytes);
				}
			}
			return playerCardView;
		}
	}
}
