using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class CurrencyDepositView {
		public CurrencyDepositView() {
		}

		public CurrencyDepositView(int creditsDepositId, DateTime depositDate, int credits, int points, decimal cash, string currencyLabel, int cmid, bool isAdminAction, PaymentProviderType paymentProviderId, string transactionKey, int applicationId, ChannelType channelId, decimal usdAmount, int? bundleId, string bundleName) {
			this.CreditsDepositId = creditsDepositId;
			this.DepositDate = depositDate;
			this.Credits = credits;
			this.Points = points;
			this.Cash = cash;
			this.CurrencyLabel = currencyLabel;
			this.Cmid = cmid;
			this.IsAdminAction = isAdminAction;
			this.PaymentProviderId = paymentProviderId;
			this.TransactionKey = transactionKey;
			this.ApplicationId = applicationId;
			this.ChannelId = channelId;
			this.UsdAmount = usdAmount;
			this.BundleId = bundleId;
			this.BundleName = bundleName;
		}

		public int CreditsDepositId { get; set; }

		public DateTime DepositDate { get; set; }

		public int Credits { get; set; }

		public int Points { get; set; }

		public decimal Cash { get; set; }

		public string CurrencyLabel { get; set; }

		public int Cmid { get; set; }

		public bool IsAdminAction { get; set; }

		public PaymentProviderType PaymentProviderId { get; set; }

		public string TransactionKey { get; set; }

		public int ApplicationId { get; set; }

		public ChannelType ChannelId { get; set; }

		public decimal UsdAmount { get; set; }

		public int? BundleId { get; set; }

		public string BundleName { get; set; }
	}
}
