using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class PointDepositsViewModel {
		public List<PointDepositView> PointDeposits { get; set; }

		public int TotalCount { get; set; }
	}
}
