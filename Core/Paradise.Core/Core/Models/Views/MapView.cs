using Paradise.Core.Types;
using System;
using System.Collections.Generic;

namespace Paradise.Core.Models.Views {
	[Serializable]
	public class MapView {
		public int MapId { get; set; }

		public string DisplayName { get; set; }

		public string Description { get; set; }

		public string SceneName { get; set; }

		public bool IsBlueBox { get; set; }

		public int RecommendedItemId { get; set; }

		public int SupportedGameModes { get; set; }

		public int SupportedItemClass { get; set; }

		public int MaxPlayers { get; set; }

		public string FileName { get; set; } // # LEGACY # //

		public Dictionary<GameModeType, MapSettings> Settings { get; set; }
	}
}
