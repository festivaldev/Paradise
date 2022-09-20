using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PointDepositsViewModelProxy {
		public static void Serialize(Stream stream, PointDepositsViewModel instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.PointDeposits != null) {
					ListProxy<PointDepositView>.Serialize(memoryStream, instance.PointDeposits, new ListProxy<PointDepositView>.Serializer<PointDepositView>(PointDepositViewProxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.TotalCount);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static PointDepositsViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PointDepositsViewModel pointDepositsViewModel = new PointDepositsViewModel();
			if ((num & 1) != 0) {
				pointDepositsViewModel.PointDeposits = ListProxy<PointDepositView>.Deserialize(bytes, new ListProxy<PointDepositView>.Deserializer<PointDepositView>(PointDepositViewProxy.Deserialize));
			}
			pointDepositsViewModel.TotalCount = Int32Proxy.Deserialize(bytes);
			return pointDepositsViewModel;
		}
	}
}
