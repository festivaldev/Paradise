using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ClanRequestDeclineView {
		public int ActionResult { get; set; }

		public int ClanRequestId { get; set; }
	}
}
