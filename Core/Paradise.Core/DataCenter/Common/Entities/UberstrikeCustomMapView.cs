using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class UberstrikeCustomMapView {
		public string Name { get; set; }
		public string FileName { get; set; }
		public int MapId { get; set; }
		public List<int> SupportedGameModes;
	}
}
