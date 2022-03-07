using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class MemberWalletView {
		public MemberWalletView() {
			this.CreditsExpiration = DateTime.Today;
			this.PointsExpiration = DateTime.Today;
		}

		public MemberWalletView(int cmid, int? credits, int? points, DateTime? creditsExpiration, DateTime? pointsExpiration) {
			if (credits == null) {
				credits = new int?(0);
			}
			if (points == null) {
				points = new int?(0);
			}
			if (creditsExpiration == null) {
				creditsExpiration = new DateTime?(DateTime.MinValue);
			}
			if (pointsExpiration == null) {
				pointsExpiration = new DateTime?(DateTime.MinValue);
			}
			this.SetMemberWallet(cmid, credits.Value, points.Value, creditsExpiration.Value, pointsExpiration.Value);
		}

		public MemberWalletView(int cmid, int credits, int points, DateTime creditsExpiration, DateTime pointsExpiration) {
			this.SetMemberWallet(cmid, credits, points, creditsExpiration, pointsExpiration);
		}

		public int Cmid { get; set; }

		public int Credits { get; set; }

		public int Points { get; set; }

		public DateTime CreditsExpiration { get; set; }

		public DateTime PointsExpiration { get; set; }

		private void SetMemberWallet(int cmid, int credits, int points, DateTime creditsExpiration, DateTime pointsExpiration) {
			this.Cmid = cmid;
			this.Credits = credits;
			this.Points = points;
			this.CreditsExpiration = creditsExpiration;
			this.PointsExpiration = pointsExpiration;
		}

		public override string ToString() {
			string text = "[Wallet: ";
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"[CMID:",
				this.Cmid,
				"][Credits:",
				this.Credits,
				"][Credits Expiration:",
				this.CreditsExpiration,
				"][Points:",
				this.Points,
				"][Points Expiration:",
				this.PointsExpiration,
				"]"
			});
			return text + "]";
		}
	}
}
