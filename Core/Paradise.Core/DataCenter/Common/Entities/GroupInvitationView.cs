using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class GroupInvitationView {
		public GroupInvitationView() {
		}

		public GroupInvitationView(int inviterCmid, int groupId, int inviteeCmid, string message) {
			this.InviterCmid = inviterCmid;
			this.InviterName = string.Empty;
			this.GroupName = string.Empty;
			this.GroupTag = string.Empty;
			this.GroupId = groupId;
			this.GroupInvitationId = 0;
			this.InviteeCmid = inviteeCmid;
			this.InviteeName = string.Empty;
			this.Message = message;
		}

		public GroupInvitationView(int inviterCmid, string inviterName, string groupName, string groupTag, int groupId, int groupInvitationId, int inviteeCmid, string inviteeName, string message) {
			this.InviterCmid = inviterCmid;
			this.InviterName = inviterName;
			this.GroupName = groupName;
			this.GroupTag = groupTag;
			this.GroupId = groupId;
			this.GroupInvitationId = groupInvitationId;
			this.InviteeCmid = inviteeCmid;
			this.InviteeName = inviteeName;
			this.Message = message;
		}

		public string InviterName { get; set; }

		public int InviterCmid { get; set; }

		public int GroupId { get; set; }

		public string GroupName { get; set; }

		public string GroupTag { get; set; }

		public int GroupInvitationId { get; set; }

		public string InviteeName { get; set; }

		public int InviteeCmid { get; set; }

		public string Message { get; set; }

		public override string ToString() {
			string text = string.Concat(new object[]
			{
				"[GroupInvitationDisplayView: [InviterCmid: ",
				this.InviterCmid,
				"][InviterName: ",
				this.InviterName,
				"]"
			});
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[GroupName: ",
				this.GroupName,
				"][GroupTag: ",
				this.GroupTag,
				"][GroupId: ",
				this.GroupId,
				"]"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[GroupInvitationId:",
				this.GroupInvitationId,
				"][InviteeCmid:",
				this.InviteeCmid,
				"][InviteeName:",
				this.InviteeName,
				"]"
			});
			return text + "[Message:" + this.Message + "]]";
		}
	}
}
