namespace Paradise.DataCenter.Common.Entities {
	public class EpinView {
		public EpinView(int epinId, string pin, bool isRedeemed, int batchId, bool isRetired) {
			this.EpinId = epinId;
			this.Pin = pin;
			this.IsRedeemed = isRedeemed;
			this.BatchId = batchId;
			this.IsRetired = isRetired;
		}

		public int EpinId { get; private set; }

		public string Pin { get; private set; }

		public bool IsRedeemed { get; private set; }

		public int BatchId { get; private set; }

		public bool IsRetired { get; private set; }
	}
}
