using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class UberstrikeMemberView {
		public UberstrikeMemberView() {
		}

		public UberstrikeMemberView(PlayerCardView playerCardView, PlayerStatisticsView playerStatisticsView) {
			this.PlayerCardView = playerCardView;
			this.PlayerStatisticsView = playerStatisticsView;
		}

		public PlayerCardView PlayerCardView { get; set; }

		public PlayerStatisticsView PlayerStatisticsView { get; set; }

		public override string ToString() {
			string str = "[Uberstrike member view: ";
			if (this.PlayerCardView != null) {
				str += this.PlayerCardView.ToString();
			} else {
				str += "null";
			}
			if (this.PlayerStatisticsView != null) {
				str += this.PlayerStatisticsView.ToString();
			} else {
				str += "null";
			}
			return str + "]";
		}
	}
}
