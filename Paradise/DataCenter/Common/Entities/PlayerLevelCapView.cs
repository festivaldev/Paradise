using System;
using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PlayerLevelCapView {
		public PlayerLevelCapView() {
		}

		public PlayerLevelCapView(int level, int xpRequired) {
			this.Level = level;
			this.XPRequired = xpRequired;
		}

		public PlayerLevelCapView(int playerLevelCapId, int level, int xpRequired) : this(level, xpRequired) {
			this.PlayerLevelCapId = playerLevelCapId;
		}

		public int PlayerLevelCapId { get; set; }

		public int Level { get; set; }

		public int XPRequired { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[PlayerLevelCapView: ");
			stringBuilder.Append("[PlayerLevelCapId: ");
			stringBuilder.Append(this.PlayerLevelCapId);
			stringBuilder.Append("][Level: ");
			stringBuilder.Append(this.Level);
			stringBuilder.Append("][XPRequired: ");
			stringBuilder.Append(this.XPRequired);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
