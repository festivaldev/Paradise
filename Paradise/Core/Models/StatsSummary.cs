using System;
using System.Collections.Generic;

namespace Paradise.Core.Models {
	[Serializable]
	public class StatsSummary {
		public string Name { get; set; }

		public int Kills { get; set; }

		public int Deaths { get; set; }

		public int Level { get; set; }

		public int Cmid { get; set; }

		public TeamID Team { get; set; }

		public Dictionary<byte, ushort> Achievements { get; set; }
	}
}
