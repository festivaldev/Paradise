using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class LuckyDrawSetUnityViewProxy {
		public static void Serialize(Stream stream, LuckyDrawSetUnityView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.CreditsAttributed);
					BooleanProxy.Serialize(memoryStream, instance.ExposeItemsToPlayers);
					Int32Proxy.Serialize(memoryStream, instance.Id);
					if (instance.ImageUrl != null) {
						StringProxy.Serialize(memoryStream, instance.ImageUrl);
					} else {
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.LuckyDrawId);
					if (instance.LuckyDrawSetItems != null) {
						ListProxy<BundleItemView>.Serialize(memoryStream, instance.LuckyDrawSetItems, new ListProxy<BundleItemView>.Serializer<BundleItemView>(BundleItemViewProxy.Serialize));
					} else {
						num |= 2;
					}
					Int32Proxy.Serialize(memoryStream, instance.PointsAttributed);
					Int32Proxy.Serialize(memoryStream, instance.SetWeight);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static LuckyDrawSetUnityView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			LuckyDrawSetUnityView luckyDrawSetUnityView = null;
			if (num != 0) {
				luckyDrawSetUnityView = new LuckyDrawSetUnityView();
				luckyDrawSetUnityView.CreditsAttributed = Int32Proxy.Deserialize(bytes);
				luckyDrawSetUnityView.ExposeItemsToPlayers = BooleanProxy.Deserialize(bytes);
				luckyDrawSetUnityView.Id = Int32Proxy.Deserialize(bytes);
				if ((num & 1) != 0) {
					luckyDrawSetUnityView.ImageUrl = StringProxy.Deserialize(bytes);
				}
				luckyDrawSetUnityView.LuckyDrawId = Int32Proxy.Deserialize(bytes);
				if ((num & 2) != 0) {
					luckyDrawSetUnityView.LuckyDrawSetItems = ListProxy<BundleItemView>.Deserialize(bytes, new ListProxy<BundleItemView>.Deserializer<BundleItemView>(BundleItemViewProxy.Deserialize));
				}
				luckyDrawSetUnityView.PointsAttributed = Int32Proxy.Deserialize(bytes);
				luckyDrawSetUnityView.SetWeight = Int32Proxy.Deserialize(bytes);
			}
			return luckyDrawSetUnityView;
		}
	}
}
