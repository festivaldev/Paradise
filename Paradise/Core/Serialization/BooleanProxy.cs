using System;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class BooleanProxy {
		public static void Serialize(Stream bytes, bool instance) {
			byte[] bytes2 = BitConverter.GetBytes(instance);
			bytes.Write(bytes2, 0, bytes2.Length);
		}

		public static bool Deserialize(Stream bytes) {
			byte[] array = new byte[1];
			bytes.Read(array, 0, 1);
			return BitConverter.ToBoolean(array, 0);
		}
	}
}
