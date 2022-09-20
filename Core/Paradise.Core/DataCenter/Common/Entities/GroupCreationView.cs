using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class GroupCreationView {
		public GroupCreationView() {
		}

		public GroupCreationView(string name, string description, string motto, string address, bool hasPicture, int applicationId, string authToken, string tag, string locale) {
			this.Name = name;
			this.Description = description;
			this.Motto = motto;
			this.Address = address;
			this.HasPicture = hasPicture;
			this.ApplicationId = applicationId;
			this.AuthToken = authToken;
			this.Tag = tag;
			this.Locale = locale;
		}

		public GroupCreationView(string name, string motto, int applicationId, string authToken, string tag, string locale) {
			this.Name = name;
			this.Description = string.Empty;
			this.Motto = motto;
			this.Address = string.Empty;
			this.HasPicture = false;
			this.ApplicationId = applicationId;
			this.AuthToken = authToken;
			this.Tag = tag;
			this.Locale = locale;
		}

		public string Name { get; set; }

		public string Description { get; set; }

		public string Motto { get; set; }

		public string Address { get; set; }

		public bool HasPicture { get; set; }

		public int ApplicationId { get; set; }

		public string AuthToken { get; set; }

		public string Tag { get; set; }

		public string Locale { get; set; }

		public int Cmid { get; set; } // # LEGACY # //

		public override string ToString() {
			string text = string.Concat(new string[]
			{
				"[GroupCreationView: [name:",
				this.Name,
				"][description:",
				this.Description,
				"][Motto:",
				this.Motto,
				"]"
			});
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[Address:",
				this.Address,
				"][Has picture:",
				this.HasPicture,
				"][Application Id:",
				this.ApplicationId,
				"][AuthToken:",
				this.AuthToken,
				"]"
			});
			text2 = text;
			return string.Concat(new string[]
			{
				text2,
				"[Tag:",
				this.Tag,
				"][Locale:",
				this.Locale,
				"]"
			});
		}
	}
}
