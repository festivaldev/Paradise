using System.IO;

namespace Paradise.Core.Serialization {
	public static class ByteProxy {
		public static void Serialize(Stream bytes, byte instance) {
			bytes.WriteByte(instance);
		}

		public static byte Deserialize(Stream bytes) {
			return (byte)bytes.ReadByte();
		}
	}
}
