namespace Paradise.DataCenter.Common.Entities {
	public class LuckyDrawSetItemView {
		public int Id { get; set; }

		public string Name { get; set; }

		public int ItemId { get; set; }

		public BuyingDurationType DurationType { get; set; }

		public int Amount { get; set; }

		public int LuckyDrawSetId { get; set; }
	}
}
