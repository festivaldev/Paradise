using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ItemTransactionView {
		public ItemTransactionView() {
		}

		public ItemTransactionView(int withdrawalId, DateTime withdrawalDate, int points, int credits, int cmid, bool isAdminAction, int itemId, BuyingDurationType duration) {
			this.WithdrawalId = withdrawalId;
			this.WithdrawalDate = withdrawalDate;
			this.Points = points;
			this.Credits = credits;
			this.Cmid = cmid;
			this.IsAdminAction = isAdminAction;
			this.ItemId = itemId;
			this.Duration = duration;
		}

		public int WithdrawalId { get; set; }

		public DateTime WithdrawalDate { get; set; }

		public int Points { get; set; }

		public int Credits { get; set; }

		public int Cmid { get; set; }

		public bool IsAdminAction { get; set; }

		public int ItemId { get; set; }

		public BuyingDurationType Duration { get; set; }
	}
}
