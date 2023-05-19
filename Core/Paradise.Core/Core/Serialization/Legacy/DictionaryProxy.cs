using System.Collections.Generic;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class DictionaryProxy<S, T> {
		public static void Serialize(Stream bytes, Dictionary<S, T> instance, DictionaryProxy<S, T>.Serializer<S> keySerialization, DictionaryProxy<S, T>.Serializer<T> valueSerialization) {
			Int32Proxy.Serialize(bytes, instance.Count);
			foreach (KeyValuePair<S, T> keyValuePair in instance) {
				keySerialization(bytes, keyValuePair.Key);
				valueSerialization(bytes, keyValuePair.Value);
			}
		}

		public static Dictionary<S, T> Deserialize(Stream bytes, DictionaryProxy<S, T>.Deserializer<S> keySerialization, DictionaryProxy<S, T>.Deserializer<T> valueSerialization) {
			int num = Int32Proxy.Deserialize(bytes);
			Dictionary<S, T> dictionary = new Dictionary<S, T>(num);
			for (int i = 0; i < num; i++) {
				dictionary.Add(keySerialization(bytes), valueSerialization(bytes));
			}
			return dictionary;
		}

		public delegate void Serializer<U>(Stream stream, U instance);

		public delegate U Deserializer<U>(Stream stream);
	}
}
