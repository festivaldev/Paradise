using System;
using System.IO;
using UnityEngine;

namespace Paradise.Core.Serialization {
	public static class QuaternionProxy {
		public static void Serialize(Stream bytes, Quaternion instance) {
			bytes.Write(BitConverter.GetBytes(instance.x), 0, 4);
			bytes.Write(BitConverter.GetBytes(instance.y), 0, 4);
			bytes.Write(BitConverter.GetBytes(instance.z), 0, 4);
			bytes.Write(BitConverter.GetBytes(instance.w), 0, 4);
		}

		public static Quaternion Deserialize(Stream bytes) {
			byte[] array = new byte[16];
			bytes.Read(array, 0, 16);
			return new Quaternion(BitConverter.ToSingle(array, 0), BitConverter.ToSingle(array, 4), BitConverter.ToSingle(array, 8), BitConverter.ToSingle(array, 12));
		}
	}
}
