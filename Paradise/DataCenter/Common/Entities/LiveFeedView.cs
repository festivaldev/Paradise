using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class LiveFeedView {
		public LiveFeedView() {
			this.Date = DateTime.UtcNow;
			this.Priority = 0;
			this.Description = string.Empty;
			this.Url = string.Empty;
			this.LivedFeedId = 0;
		}

		public LiveFeedView(DateTime date, int priority, string description, string url, int liveFeedId) {
			this.Date = date;
			this.Priority = priority;
			this.Description = description;
			this.Url = url;
			this.LivedFeedId = liveFeedId;
		}

		public DateTime Date { get; set; }

		public int Priority { get; set; }

		public string Description { get; set; }

		public string Url { get; set; }

		public int LivedFeedId { get; set; }
	}
}
