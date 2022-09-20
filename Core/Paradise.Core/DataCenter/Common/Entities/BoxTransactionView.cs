using System;

namespace Paradise.DataCenter.Common.Entities {
	public class BoxTransactionView {
		public int Id { get; set; }

		public BoxType BoxType { get; set; }

		public BundleCategoryType Category { get; set; }

		public int BoxId { get; set; }

		public int Cmid { get; set; }

		public DateTime TransactionDate { get; set; }

		public bool IsAdmin { get; set; }

		public int CreditPrice { get; set; }

		public int PointPrice { get; set; }

		public int TotalCreditsAttributed { get; set; }

		public int TotalPointsAttributed { get; set; }
	}
}
