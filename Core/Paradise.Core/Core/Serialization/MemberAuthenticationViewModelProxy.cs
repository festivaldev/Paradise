using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MemberAuthenticationViewModelProxy {
		public static void Serialize(Stream stream, MemberAuthenticationViewModel instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				EnumProxy<MemberAuthenticationResult>.Serialize(memoryStream, instance.MemberAuthenticationResult);
				if (instance.MemberView != null) {
					MemberViewProxy.Serialize(memoryStream, instance.MemberView);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static MemberAuthenticationViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MemberAuthenticationViewModel memberAuthenticationViewModel = new MemberAuthenticationViewModel();
			memberAuthenticationViewModel.MemberAuthenticationResult = EnumProxy<MemberAuthenticationResult>.Deserialize(bytes);
			if ((num & 1) != 0) {
				memberAuthenticationViewModel.MemberView = MemberViewProxy.Deserialize(bytes);
			}
			return memberAuthenticationViewModel;
		}
	}
}
