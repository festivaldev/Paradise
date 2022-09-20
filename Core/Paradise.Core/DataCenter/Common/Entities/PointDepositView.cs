using System;

namespace Paradise.DataCenter.Common.Entities {
	[Serializable]
	public class PointDepositView {
		public PointDepositView() {
		}

		public PointDepositView(int pointDepositId, DateTime depositDate, int points, int cmid, bool isAdminAction, PointsDepositType despositType) {
			this.PointDepositId = pointDepositId;
			this.DepositDate = depositDate;
			this.Points = points;
			this.Cmid = cmid;
			this.IsAdminAction = isAdminAction;
			this.DepositType = despositType;
		}

		public int PointDepositId { get; set; }

		public DateTime DepositDate { get; set; }

		public int Points { get; set; }

		public int Cmid { get; set; }

		public bool IsAdminAction { get; set; }

		public PointsDepositType DepositType { get; set; }
	}
}
