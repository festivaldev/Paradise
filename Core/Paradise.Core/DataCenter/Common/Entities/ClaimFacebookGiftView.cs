using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ClaimFacebookGiftView {
		public ClaimFacebookGiftView() {
		}

		public ClaimFacebookGiftView(ClaimFacebookGiftResult _claimResult, int? _itemId) {
			this.ClaimResult = _claimResult;
			this.ItemId = _itemId;
		}

		public ClaimFacebookGiftResult ClaimResult { get; set; }

		public int? ItemId { get; set; }
	}
}
