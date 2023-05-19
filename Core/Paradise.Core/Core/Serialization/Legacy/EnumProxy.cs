using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class EnumProxy<T> {
		public static void Serialize(Stream bytes, T instance) {
			byte[] bytes2 = BitConverter.GetBytes(Convert.ToInt32(instance));
			bytes.Write(bytes2, 0, bytes2.Length);
		}

		public static T Deserialize(Stream bytes) {
			byte[] array = new byte[4];
			bytes.Read(array, 0, 4);
			return (T)((object)Enum.ToObject(typeof(T), BitConverter.ToInt32(array, 0)));
		}
	}
}
