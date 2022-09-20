using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class BasicClanView {
		public BasicClanView() {
		}

		public BasicClanView(int groupId, int membersCount, string description, string name, string motto, string address, DateTime foundingDate, string picture, GroupType type, DateTime lastUpdated, string tag, int membersLimit, GroupColor colorStyle, GroupFontStyle fontStyle, int applicationId, int ownerCmid, string ownerName) {
			this.SetClan(groupId, membersCount, description, name, motto, address, foundingDate, picture, type, lastUpdated, tag, membersLimit, colorStyle, fontStyle, applicationId, ownerCmid, ownerName);
		}

		public int GroupId { get; set; }

		public int MembersCount { get; set; }

		public string Description { get; set; }

		public string Name { get; set; }

		public string Motto { get; set; }

		public string Address { get; set; }

		public DateTime FoundingDate { get; set; }

		public string Picture { get; set; }

		public GroupType Type { get; set; }

		public DateTime LastUpdated { get; set; }

		public string Tag { get; set; }

		public int MembersLimit { get; set; }

		public GroupColor ColorStyle { get; set; }

		public GroupFontStyle FontStyle { get; set; }

		public int ApplicationId { get; set; }

		public int OwnerCmid { get; set; }

		public string OwnerName { get; set; }

		public void SetClan(int groupId, int membersCount, string description, string name, string motto, string address, DateTime foundingDate, string picture, GroupType type, DateTime lastUpdated, string tag, int membersLimit, GroupColor colorStyle, GroupFontStyle fontStyle, int applicationId, int ownerCmid, string ownerName) {
			this.GroupId = groupId;
			this.MembersCount = membersCount;
			this.Description = description;
			this.Name = name;
			this.Motto = motto;
			this.Address = address;
			this.FoundingDate = foundingDate;
			this.Picture = picture;
			this.Type = type;
			this.LastUpdated = lastUpdated;
			this.Tag = tag;
			this.MembersLimit = membersLimit;
			this.ColorStyle = colorStyle;
			this.FontStyle = fontStyle;
			this.ApplicationId = applicationId;
			this.OwnerCmid = ownerCmid;
			this.OwnerName = ownerName;
		}

		public override string ToString() {
			string text = string.Concat(new object[]
			{
				"[Clan: [Id: ",
				this.GroupId,
				"][Members count: ",
				this.MembersCount,
				"][Description: ",
				this.Description,
				"]"
			});
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				"[Name: ",
				this.Name,
				"][Motto: ",
				this.Name,
				"][Address: ",
				this.Address,
				"]"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Creation date: ",
				this.FoundingDate,
				"][Picture: ",
				this.Picture,
				"][Type: ",
				this.Type,
				"][Last updated: ",
				this.LastUpdated,
				"]"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Tag: ",
				this.Tag,
				"][Members limit: ",
				this.MembersLimit,
				"][Color style: ",
				this.ColorStyle,
				"][Font style: ",
				this.FontStyle,
				"]"
			});
			text2 = text;
			return string.Concat(new object[]
			{
				text2,
				"[Application Id: ",
				this.ApplicationId,
				"][Owner Cmid: ",
				this.OwnerCmid,
				"][Owner name: ",
				this.OwnerName,
				"]]"
			});
		}
	}
}
