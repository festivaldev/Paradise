using System;

namespace UberStrike.Core.Models.Views {
	[Serializable]
	public class ParadiseMapView : MapView {
		public string FileName { get; set; }
		public ParadiseMapView() { }

		public ParadiseMapView(MapView map) {
			Description = map.Description;
			DisplayName = map.DisplayName;
			IsBlueBox = map.IsBlueBox;
			MapId = map.MapId;
			MaxPlayers = map.MaxPlayers;
			RecommendedItemId = map.RecommendedItemId;
			SceneName = map.SceneName;
			Settings = map.Settings;
			SupportedGameModes = map.SupportedGameModes;
			SupportedItemClass = map.SupportedItemClass;
		}
	}
}
