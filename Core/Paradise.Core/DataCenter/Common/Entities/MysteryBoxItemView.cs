namespace Paradise.DataCenter.Common.Entities {
	public class MysteryBoxItemView {
		public int Id { get; set; }

		public int ItemId { get; set; }

		public string Name { get; set; }

		public int Amount { get; set; }

		public BuyingDurationType DurationType { get; set; }

		public int ItemWeight { get; set; }

		public int MysteryBoxId { get; set; }
	}
}
