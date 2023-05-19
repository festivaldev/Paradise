using Paradise.Core.Models.Views;
using Paradise.Core.ViewModel;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class UberstrikeLevelViewModelProxy {
		public static void Serialize(Stream stream, UberstrikeLevelViewModel instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					if (instance.Maps != null) {
						ListProxy<MapView>.Serialize(memoryStream, instance.Maps, new ListProxy<MapView>.Serializer<MapView>(MapViewProxy.Serialize));
					} else {
						num |= 1;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static UberstrikeLevelViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			UberstrikeLevelViewModel uberstrikeLevelViewModel = null;
			if (num != 0) {
				uberstrikeLevelViewModel = new UberstrikeLevelViewModel();
				if ((num & 1) != 0) {
					uberstrikeLevelViewModel.Maps = ListProxy<MapView>.Deserialize(bytes, new ListProxy<MapView>.Deserializer<MapView>(MapViewProxy.Deserialize));
				}
			}
			return uberstrikeLevelViewModel;
		}
	}
}
