using System;
using System.IO;
using UnityEngine;

namespace Paradise.Core.Serialization {
	public static class Vector3Proxy {
		public static void Serialize(Stream bytes, Vector3 instance) {
			bytes.Write(BitConverter.GetBytes(instance.x), 0, 4);
			bytes.Write(BitConverter.GetBytes(instance.y), 0, 4);
			bytes.Write(BitConverter.GetBytes(instance.z), 0, 4);
		}

		public static Vector3 Deserialize(Stream bytes) {
			byte[] array = new byte[12];
			bytes.Read(array, 0, 12);
			return new Vector3(BitConverter.ToSingle(array, 0), BitConverter.ToSingle(array, 4), BitConverter.ToSingle(array, 8));
		}
	}
}
