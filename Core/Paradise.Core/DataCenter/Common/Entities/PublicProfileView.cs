using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PublicProfileView {
		public PublicProfileView() {
			this.Cmid = 0;
			this.Name = string.Empty;
			this.IsChatDisabled = false;
			this.AccessLevel = MemberAccessLevel.Default;
			this.GroupTag = string.Empty;
			this.LastLoginDate = DateTime.UtcNow;
			this.EmailAddressStatus = EmailAddressStatus.Unverified;
			this.FacebookId = string.Empty;
		}

		public PublicProfileView(int cmid, string name, MemberAccessLevel accesLevel, bool isChatDisabled, DateTime lastLoginDate, EmailAddressStatus emailAddressStatus, string facebookId) {
			this.SetPublicProfile(cmid, name, accesLevel, isChatDisabled, string.Empty, lastLoginDate, emailAddressStatus, facebookId);
		}

		public PublicProfileView(int cmid, string name, MemberAccessLevel accesLevel, bool isChatDisabled, string groupTag, DateTime lastLoginDate, EmailAddressStatus emailAddressStatus, string facebookId) {
			this.SetPublicProfile(cmid, name, accesLevel, isChatDisabled, groupTag, lastLoginDate, emailAddressStatus, facebookId);
		}

		public int Cmid { get; set; }

		public string Name { get; set; }

		public bool IsChatDisabled { get; set; }

		public MemberAccessLevel AccessLevel { get; set; }

		public string GroupTag { get; set; }

		public DateTime LastLoginDate { get; set; }

		public EmailAddressStatus EmailAddressStatus { get; set; }

		public string FacebookId { get; set; }

		private void SetPublicProfile(int cmid, string name, MemberAccessLevel accesLevel, bool isChatDisabled, string groupTag, DateTime lastLoginDate, EmailAddressStatus emailAddressStatus, string facebookId) {
			this.Cmid = cmid;
			this.Name = name;
			this.AccessLevel = accesLevel;
			this.IsChatDisabled = isChatDisabled;
			this.GroupTag = groupTag;
			this.LastLoginDate = lastLoginDate;
			this.EmailAddressStatus = emailAddressStatus;
			this.FacebookId = facebookId;
		}

		public override string ToString() {
			string text = "[Public profile: ";
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Member name:",
				this.Name,
				"][CMID:",
				this.Cmid,
				"][Is banned from chat: ",
				this.IsChatDisabled,
				"]"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Access level:",
				this.AccessLevel,
				"][Group tag: ",
				this.GroupTag,
				"][Last login date: ",
				this.LastLoginDate,
				"]]"
			});
			text2 = text;
			return string.Concat(new object[]
			{
				text2,
				"[EmailAddressStatus:",
				this.EmailAddressStatus,
				"][FacebookId: ",
				this.FacebookId,
				"]"
			});
		}
	}
}
