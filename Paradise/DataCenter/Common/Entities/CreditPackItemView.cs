namespace Paradise.DataCenter.Common.Entities {
	public class CreditPackItemView {
		public int CreditPackId { get; set; }

		public int ItemId { get; set; }

		public BuyingDurationType Duration { get; set; }

		public ItemView ItemView { get; set; }
	}
}
