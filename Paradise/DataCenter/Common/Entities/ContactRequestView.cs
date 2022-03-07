using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ContactRequestView {
		public ContactRequestView() {
		}

		public ContactRequestView(int initiatorCmid, int receiverCmid, string initiatorMessage) {
			this.InitiatorCmid = initiatorCmid;
			this.ReceiverCmid = receiverCmid;
			this.InitiatorMessage = initiatorMessage;
		}

		public ContactRequestView(int requestID, int initiatorCmid, string initiatorName, int receiverCmid, string initiatorMessage, ContactRequestStatus status, DateTime sentDate) {
			this.SetContactRequest(requestID, initiatorCmid, initiatorName, receiverCmid, initiatorMessage, status, sentDate);
		}

		public int RequestId { get; set; }

		public int InitiatorCmid { get; set; }

		public string InitiatorName { get; set; }

		public int ReceiverCmid { get; set; }

		public string InitiatorMessage { get; set; }

		public ContactRequestStatus Status { get; set; }

		public DateTime SentDate { get; set; }

		public void SetContactRequest(int requestID, int initiatorCmid, string initiatorName, int receiverCmid, string initiatorMessage, ContactRequestStatus status, DateTime sentDate) {
			this.RequestId = requestID;
			this.InitiatorCmid = initiatorCmid;
			this.InitiatorName = initiatorName;
			this.ReceiverCmid = receiverCmid;
			this.InitiatorMessage = initiatorMessage;
			this.Status = status;
			this.SentDate = sentDate;
		}

		public override string ToString() {
			string text = string.Concat(new object[]
			{
				"[Request contact: [Request ID: ",
				this.RequestId,
				"][Initiator Cmid :",
				this.InitiatorCmid,
				"][Initiator Name:",
				this.InitiatorName,
				"][Receiver Cmid: ",
				this.ReceiverCmid,
				"]"
			});
			string text2 = text;
			return string.Concat(new object[]
			{
				text2,
				"[Initiator Message: ",
				this.InitiatorMessage,
				"][Status: ",
				this.Status,
				"][Sent Date: ",
				this.SentDate,
				"]]"
			});
		}
	}
}
