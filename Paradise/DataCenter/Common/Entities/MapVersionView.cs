using System;

namespace Paradise.DataCenter.Common.Entities {
	public class MapVersionView {
		public MapVersionView(string fileName, DateTime lastUpdatedDate) {
			this.FileName = fileName;
			this.LastUpdatedDate = lastUpdatedDate;
		}

		public string FileName { get; private set; }

		public DateTime LastUpdatedDate { get; set; }
	}
}
