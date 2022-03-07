using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ClanCreationReturnView {
		public int ResultCode { get; set; }

		public ClanView ClanView { get; set; }
	}
}
