using System.IO;
using System.Text;

namespace Paradise.Core.Serialization {
	public static class StringProxy {
		public static void Serialize(Stream bytes, string instance) {
			if (string.IsNullOrEmpty(instance)) {
				UShortProxy.Serialize(bytes, 0);
			} else {
				UShortProxy.Serialize(bytes, (ushort)instance.Length);
				byte[] bytes2 = Encoding.Unicode.GetBytes(instance);
				bytes.Write(bytes2, 0, bytes2.Length);
			}
		}

		public static string Deserialize(Stream bytes) {
			ushort num = UShortProxy.Deserialize(bytes);
			if (num > 0) {
				byte[] array = new byte[(int)(num * 2)];
				bytes.Read(array, 0, array.Length);
				return Encoding.Unicode.GetString(array, 0, array.Length);
			}
			return string.Empty;
		}
	}
}
