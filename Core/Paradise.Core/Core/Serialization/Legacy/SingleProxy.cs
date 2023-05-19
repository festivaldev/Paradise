using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class SingleProxy {
		public static void Serialize(Stream bytes, float instance) {
			byte[] bytes2 = BitConverter.GetBytes(instance);
			bytes.Write(bytes2, 0, bytes2.Length);
		}

		public static float Deserialize(Stream bytes) {
			byte[] array = new byte[4];
			bytes.Read(array, 0, 4);
			return BitConverter.ToSingle(array, 0);
		}
	}
}
