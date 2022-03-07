using Paradise.Core.ViewModel;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class UberstrikeUserViewModelProxy {
		public static void Serialize(Stream stream, UberstrikeUserViewModel instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.CmuneMemberView != null) {
					MemberViewProxy.Serialize(memoryStream, instance.CmuneMemberView);
				} else {
					num |= 1;
				}
				if (instance.UberstrikeMemberView != null) {
					UberstrikeMemberViewProxy.Serialize(memoryStream, instance.UberstrikeMemberView);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static UberstrikeUserViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			UberstrikeUserViewModel uberstrikeUserViewModel = new UberstrikeUserViewModel();
			if ((num & 1) != 0) {
				uberstrikeUserViewModel.CmuneMemberView = MemberViewProxy.Deserialize(bytes);
			}
			if ((num & 2) != 0) {
				uberstrikeUserViewModel.UberstrikeMemberView = UberstrikeMemberViewProxy.Deserialize(bytes);
			}
			return uberstrikeUserViewModel;
		}
	}
}
