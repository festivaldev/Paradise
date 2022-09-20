using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ContactGroupView {
		public ContactGroupView() {
			this.Contacts = new List<PublicProfileView>(0);
			this.GroupName = string.Empty;
		}

		public ContactGroupView(int groupID, string groupName, List<PublicProfileView> contacts) {
			this.GroupId = groupID;
			this.GroupName = groupName;
			this.Contacts = contacts;
		}

		public int GroupId { get; set; }

		public string GroupName { get; set; }

		public List<PublicProfileView> Contacts { get; set; }

		public override string ToString() {
			string text = string.Concat(new object[]
			{
				"[Contact group: [Group ID: ",
				this.GroupId,
				"][Group Name :",
				this.GroupName,
				"][Contacts: "
			});
			foreach (PublicProfileView publicProfileView in this.Contacts) {
				text = text + "[Contact: " + publicProfileView.ToString() + "]";
			}
			text += "]]";
			return text;
		}
	}
}
