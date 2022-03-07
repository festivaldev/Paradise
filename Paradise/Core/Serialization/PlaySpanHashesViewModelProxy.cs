using Paradise.Core.ViewModel;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PlaySpanHashesViewModelProxy {
		public static void Serialize(Stream stream, PlaySpanHashesViewModel instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.Hashes != null) {
					DictionaryProxy<decimal, string>.Serialize(memoryStream, instance.Hashes, new DictionaryProxy<decimal, string>.Serializer<decimal>(DecimalProxy.Serialize), new DictionaryProxy<decimal, string>.Serializer<string>(StringProxy.Serialize));
				} else {
					num |= 1;
				}
				if (instance.MerchTrans != null) {
					StringProxy.Serialize(memoryStream, instance.MerchTrans);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static PlaySpanHashesViewModel Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PlaySpanHashesViewModel playSpanHashesViewModel = new PlaySpanHashesViewModel();
			if ((num & 1) != 0) {
				playSpanHashesViewModel.Hashes = DictionaryProxy<decimal, string>.Deserialize(bytes, new DictionaryProxy<decimal, string>.Deserializer<decimal>(DecimalProxy.Deserialize), new DictionaryProxy<decimal, string>.Deserializer<string>(StringProxy.Deserialize));
			}
			if ((num & 2) != 0) {
				playSpanHashesViewModel.MerchTrans = StringProxy.Deserialize(bytes);
			}
			return playSpanHashesViewModel;
		}
	}
}
