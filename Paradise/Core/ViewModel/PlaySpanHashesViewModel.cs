using System;
using System.Collections.Generic;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class PlaySpanHashesViewModel {
		public string MerchTrans { get; set; }

		public Dictionary<decimal, string> Hashes { get; set; }
	}
}
