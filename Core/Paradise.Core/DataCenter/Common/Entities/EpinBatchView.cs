using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	public class EpinBatchView {
		public EpinBatchView(int batchId, int applicationId, PaymentProviderType epinProvider, int amount, int creditAmount, DateTime batchDate, bool isAdmin, bool isRetired, List<EpinView> epins) {
			this.BatchId = batchId;
			this.ApplicationId = applicationId;
			this.EpinProvider = epinProvider;
			this.Amount = amount;
			this.CreditAmount = creditAmount;
			this.BatchDate = batchDate;
			this.IsAdmin = isAdmin;
			this.Epins = epins;
			this.IsRetired = isRetired;
		}

		public int BatchId { get; private set; }

		public int ApplicationId { get; private set; }

		public PaymentProviderType EpinProvider { get; private set; }

		public int Amount { get; private set; }

		public int CreditAmount { get; private set; }

		public DateTime BatchDate { get; private set; }

		public bool IsAdmin { get; private set; }

		public bool IsRetired { get; private set; }

		public List<EpinView> Epins { get; private set; }
	}
}
