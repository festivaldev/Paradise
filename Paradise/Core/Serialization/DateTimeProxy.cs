using System;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class DateTimeProxy {
		public static void Serialize(Stream bytes, DateTime instance) {
			byte[] bytes2 = BitConverter.GetBytes(instance.Ticks);
			bytes.Write(bytes2, 0, bytes2.Length);
		}

		public static DateTime Deserialize(Stream bytes) {
			byte[] array = new byte[8];
			bytes.Read(array, 0, 8);
			return new DateTime(BitConverter.ToInt64(array, 0));
		}
	}
}
