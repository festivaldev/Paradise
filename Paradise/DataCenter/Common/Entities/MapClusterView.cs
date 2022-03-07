using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	public class MapClusterView {
		public MapClusterView(string appVersion, List<MapInfoView> maps) {
			this.ApplicationVersion = appVersion;
			this.Maps = maps;
		}

		public string ApplicationVersion { get; private set; }

		public List<MapInfoView> Maps { get; private set; }
	}
}
