using System.Collections.Generic;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class ListProxy<T> {
		public static void Serialize(Stream bytes, ICollection<T> instance, ListProxy<T>.Serializer<T> serialization) {
			UShortProxy.Serialize(bytes, (ushort)instance.Count);
			foreach (T instance2 in instance) {
				serialization(bytes, instance2);
			}
		}

		public static List<T> Deserialize(Stream bytes, ListProxy<T>.Deserializer<T> serialization) {
			ushort num = UShortProxy.Deserialize(bytes);
			List<T> list = new List<T>((int)num);
			for (int i = 0; i < (int)num; i++) {
				list.Add(serialization(bytes));
			}
			return list;
		}

		public delegate void Serializer<U>(Stream stream, U instance);

		public delegate U Deserializer<U>(Stream stream);
	}
}
