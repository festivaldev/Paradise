﻿namespace Paradise.Core.Models {
	public class RoomData {
		public string Guid { get; set; }

		public string Name { get; set; }

		public ConnectionAddress Server { get; set; }

		public int Number { get; set; }

		public bool IsPasswordProtected { get; set; }
	}
}
