using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class BugView {
		public BugView() {
		}

		public BugView(string subject, string content) {
			this.Subject = subject.Trim();
			this.Content = content.Trim();
		}

		public string Content { get; set; }

		public string Subject { get; set; }

		public override string ToString() {
			return string.Concat(new string[]
			{
				"[Bug: [Subject: ",
				this.Subject,
				"][Content :",
				this.Content,
				"]]"
			});
		}
	}
}
