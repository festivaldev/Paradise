using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ItemInventoryView {
		public ItemInventoryView() {
		}

		public ItemInventoryView(int itemId, DateTime? expirationDate, int amountRemaining) {
			this.ItemId = itemId;
			this.ExpirationDate = expirationDate;
			this.AmountRemaining = amountRemaining;
		}

		public ItemInventoryView(int itemId, DateTime? expirationDate, int amountRemaining, int cmid) : this(itemId, expirationDate, amountRemaining) {
			this.Cmid = cmid;
		}

		public int Cmid { get; set; }

		public int ItemId { get; set; }

		public DateTime? ExpirationDate { get; set; }

		public int AmountRemaining { get; set; }

		public override string ToString() {
			string text = "[LiveInventoryView: ";
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Item Id: ",
				this.ItemId,
				"]"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Expiration date: ",
				this.ExpirationDate,
				"]"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Amount remaining:",
				this.AmountRemaining,
				"]"
			});
			return text + "]";
		}
	}
}
