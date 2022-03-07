using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class CurrencyDepositsViewModel {
		public List<CurrencyDepositView> CurrencyDeposits { get; set; }

		public int TotalCount { get; set; }
	}
}
