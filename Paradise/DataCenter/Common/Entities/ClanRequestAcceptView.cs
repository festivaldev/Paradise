using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ClanRequestAcceptView {
		public int ActionResult { get; set; }

		public int ClanRequestId { get; set; }

		public ClanView ClanView { get; set; }
	}
}
