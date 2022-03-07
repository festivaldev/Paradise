using System;

namespace Paradise.Core.Models {
	[Serializable]
	public class GameRoom {
		public ConnectionAddress Server { get; set; }

		public int Number { get; set; }

		public int MapId { get; set; }
	}
}
