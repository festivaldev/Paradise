using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class MessageThreadView {
		public int ThreadId { get; set; }

		public string ThreadName { get; set; }

		public bool HasNewMessages { get; set; }

		public int MessageCount { get; set; }

		public string LastMessagePreview { get; set; }

		public DateTime LastUpdate { get; set; }
	}
}
