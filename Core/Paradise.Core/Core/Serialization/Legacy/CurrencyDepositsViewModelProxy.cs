using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class CurrencyDepositsViewModelProxy {
		public static void Serialize(Stream stream, CurrencyDepositsViewModel instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					if (instance.CurrencyDeposits != null) {
						ListProxy<CurrencyDepositView>.Serialize(memoryStream, instance.CurrencyDeposits, new ListProxy<CurrencyDepositView>.Serializer<CurrencyDepositView>(CurrencyDepositViewProxy.Serialize));
					} else {
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.TotalCount);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static CurrencyDepositsViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			CurrencyDepositsViewModel currencyDepositsViewModel = null;
			if (num != 0) {
				currencyDepositsViewModel = new CurrencyDepositsViewModel();
				if ((num & 1) != 0) {
					currencyDepositsViewModel.CurrencyDeposits = ListProxy<CurrencyDepositView>.Deserialize(bytes, new ListProxy<CurrencyDepositView>.Deserializer<CurrencyDepositView>(CurrencyDepositViewProxy.Deserialize));
				}
				currencyDepositsViewModel.TotalCount = Int32Proxy.Deserialize(bytes);
			}
			return currencyDepositsViewModel;
		}
	}
}
