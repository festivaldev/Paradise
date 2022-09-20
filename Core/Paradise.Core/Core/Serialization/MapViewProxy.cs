using Paradise.Core.Models.Views;
using Paradise.Core.Types;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MapViewProxy {
		public static void Serialize(Stream stream, MapView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.Description != null) {
					StringProxy.Serialize(memoryStream, instance.Description);
				} else {
					num |= 1;
				}
				if (instance.DisplayName != null) {
					StringProxy.Serialize(memoryStream, instance.DisplayName);
				} else {
					num |= 2;
				}
				BooleanProxy.Serialize(memoryStream, instance.IsBlueBox);
				Int32Proxy.Serialize(memoryStream, instance.MapId);
				Int32Proxy.Serialize(memoryStream, instance.MaxPlayers);
				Int32Proxy.Serialize(memoryStream, instance.RecommendedItemId);
				if (instance.SceneName != null) {
					StringProxy.Serialize(memoryStream, instance.SceneName);
				} else {
					num |= 4;
				}
				if (instance.Settings != null) {
					DictionaryProxy<GameModeType, MapSettings>.Serialize(memoryStream, instance.Settings, new DictionaryProxy<GameModeType, MapSettings>.Serializer<GameModeType>(EnumProxy<GameModeType>.Serialize), new DictionaryProxy<GameModeType, MapSettings>.Serializer<MapSettings>(MapSettingsProxy.Serialize));
				} else {
					num |= 8;
				}
				Int32Proxy.Serialize(memoryStream, instance.SupportedGameModes);
				Int32Proxy.Serialize(memoryStream, instance.SupportedItemClass);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static MapView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MapView mapView = new MapView();
			if ((num & 1) != 0) {
				mapView.Description = StringProxy.Deserialize(bytes);
			}
			if ((num & 2) != 0) {
				mapView.DisplayName = StringProxy.Deserialize(bytes);
			}
			mapView.IsBlueBox = BooleanProxy.Deserialize(bytes);
			mapView.MapId = Int32Proxy.Deserialize(bytes);
			mapView.MaxPlayers = Int32Proxy.Deserialize(bytes);
			mapView.RecommendedItemId = Int32Proxy.Deserialize(bytes);
			if ((num & 4) != 0) {
				mapView.SceneName = StringProxy.Deserialize(bytes);
			}
			if ((num & 8) != 0) {
				mapView.Settings = DictionaryProxy<GameModeType, MapSettings>.Deserialize(bytes, new DictionaryProxy<GameModeType, MapSettings>.Deserializer<GameModeType>(EnumProxy<GameModeType>.Deserialize), new DictionaryProxy<GameModeType, MapSettings>.Deserializer<MapSettings>(MapSettingsProxy.Deserialize));
			}
			mapView.SupportedGameModes = Int32Proxy.Deserialize(bytes);
			mapView.SupportedItemClass = Int32Proxy.Deserialize(bytes);
			return mapView;
		}
	}
}
