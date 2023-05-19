using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class ArrayProxy<T> {
		public static void Serialize(Stream bytes, T[] instance, Action<Stream, T> serialization) {
			UShortProxy.Serialize(bytes, (ushort)instance.Length);
			foreach (T arg in instance) {
				serialization(bytes, arg);
			}
		}

		public static T[] Deserialize(Stream bytes, ArrayProxy<T>.Deserializer<T> serialization) {
			ushort num = UShortProxy.Deserialize(bytes);
			T[] array = new T[(int)num];
			for (int i = 0; i < (int)num; i++) {
				array[i] = serialization(bytes);
			}
			return array;
		}

		public delegate void Serializer<U>(Stream stream, U instance);

		public delegate U Deserializer<U>(Stream stream);
	}
}
