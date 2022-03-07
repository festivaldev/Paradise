using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class AccountCompletionResultViewProxy {
		public static void Serialize(Stream stream, AccountCompletionResultView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.ItemsAttributed != null) {
					DictionaryProxy<int, int>.Serialize(memoryStream, instance.ItemsAttributed, new DictionaryProxy<int, int>.Serializer<int>(Int32Proxy.Serialize), new DictionaryProxy<int, int>.Serializer<int>(Int32Proxy.Serialize));
				} else {
					num |= 1;
				}
				if (instance.NonDuplicateNames != null) {
					ListProxy<string>.Serialize(memoryStream, instance.NonDuplicateNames, new ListProxy<string>.Serializer<string>(StringProxy.Serialize));
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(memoryStream, instance.Result);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static AccountCompletionResultView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			AccountCompletionResultView accountCompletionResultView = new AccountCompletionResultView();
			if ((num & 1) != 0) {
				accountCompletionResultView.ItemsAttributed = DictionaryProxy<int, int>.Deserialize(bytes, new DictionaryProxy<int, int>.Deserializer<int>(Int32Proxy.Deserialize), new DictionaryProxy<int, int>.Deserializer<int>(Int32Proxy.Deserialize));
			}
			if ((num & 2) != 0) {
				accountCompletionResultView.NonDuplicateNames = ListProxy<string>.Deserialize(bytes, new ListProxy<string>.Deserializer<string>(StringProxy.Deserialize));
			}
			accountCompletionResultView.Result = Int32Proxy.Deserialize(bytes);
			return accountCompletionResultView;
		}
	}
}
