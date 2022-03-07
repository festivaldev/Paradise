using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PrivateMessageView {
		public int PrivateMessageId { get; set; }

		public int FromCmid { get; set; }

		public string FromName { get; set; }

		public int ToCmid { get; set; }

		public DateTime DateSent { get; set; }

		public string ContentText { get; set; }

		public bool IsRead { get; set; }

		public bool HasAttachment { get; set; }

		public bool IsDeletedBySender { get; set; }

		public bool IsDeletedByReceiver { get; set; }

		public override string ToString() {
			string text = "[Private Message: ";
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[ID:",
				this.PrivateMessageId,
				"][From:",
				this.FromCmid,
				"][To:",
				this.ToCmid,
				"][Date:",
				this.DateSent,
				"]["
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Content:",
				this.ContentText,
				"][Is Read:",
				this.IsRead,
				"][Has attachment:",
				this.HasAttachment,
				"][Is deleted by sender:",
				this.IsDeletedBySender,
				"][Is deleted by receiver:",
				this.IsDeletedByReceiver,
				"]"
			});
			return text + "]";
		}
	}
}
