using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class RegisterClientApplicationViewModelProxy {
		public static void Serialize(Stream stream, RegisterClientApplicationViewModel instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					if (instance.ItemsAttributed != null) {
						ListProxy<int>.Serialize(memoryStream, instance.ItemsAttributed, new ListProxy<int>.Serializer<int>(Int32Proxy.Serialize));
					} else {
						num |= 1;
					}
					EnumProxy<ApplicationRegistrationResult>.Serialize(memoryStream, instance.Result);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static RegisterClientApplicationViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			RegisterClientApplicationViewModel registerClientApplicationViewModel = null;
			if (num != 0) {
				registerClientApplicationViewModel = new RegisterClientApplicationViewModel();
				if ((num & 1) != 0) {
					registerClientApplicationViewModel.ItemsAttributed = ListProxy<int>.Deserialize(bytes, new ListProxy<int>.Deserializer<int>(Int32Proxy.Deserialize));
				}
				registerClientApplicationViewModel.Result = EnumProxy<ApplicationRegistrationResult>.Deserialize(bytes);
			}
			return registerClientApplicationViewModel;
		}
	}
}
