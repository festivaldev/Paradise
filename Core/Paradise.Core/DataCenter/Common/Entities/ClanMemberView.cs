using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class ClanMemberView {
		public ClanMemberView() {
		}

		public ClanMemberView(string name, int cmid, GroupPosition position, DateTime joiningDate, DateTime lastLogin) {
			this.Cmid = cmid;
			this.Name = name;
			this.Position = position;
			this.JoiningDate = joiningDate;
			this.Lastlogin = lastLogin;
		}

		public string Name { get; set; }

		public int Cmid { get; set; }

		public GroupPosition Position { get; set; }

		public DateTime JoiningDate { get; set; }

		public DateTime Lastlogin { get; set; }

		public override string ToString() {
			return string.Concat(new object[]
			{
				"[Clan member: [Name: ",
				this.Name,
				"][Cmid: ",
				this.Cmid,
				"][Position: ",
				this.Position,
				"][JoiningDate: ",
				this.JoiningDate,
				"][Lastlogin: ",
				this.Lastlogin,
				"]]"
			});
		}
	}
}
