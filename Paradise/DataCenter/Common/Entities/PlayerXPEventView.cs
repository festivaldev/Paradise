using System;
using System.Text;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PlayerXPEventView {
		public PlayerXPEventView() {
		}

		public PlayerXPEventView(string name, decimal xpMultiplier) {
			this.Name = name;
			this.XPMultiplier = xpMultiplier;
		}

		public PlayerXPEventView(int playerXPEventId, string name, decimal xpMultiplier) : this(name, xpMultiplier) {
			this.PlayerXPEventId = playerXPEventId;
		}

		public int PlayerXPEventId { get; set; }

		public string Name { get; set; }

		public decimal XPMultiplier { get; set; }

		public override string ToString() {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[PlayerXPEventView: ");
			stringBuilder.Append("[PlayerXPEventId: ");
			stringBuilder.Append(this.PlayerXPEventId);
			stringBuilder.Append("][Name: ");
			stringBuilder.Append(this.Name);
			stringBuilder.Append("][XPMultiplier: ");
			stringBuilder.Append(this.XPMultiplier);
			stringBuilder.Append("]]");
			return stringBuilder.ToString();
		}
	}
}
