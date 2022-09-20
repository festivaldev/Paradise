using System;
using System.IO;
using UnityEngine;

namespace Paradise.Core.Serialization {
	public static class ColorProxy {
		public static void Serialize(Stream bytes, Color instance) {
			bytes.Write(BitConverter.GetBytes(instance.r), 0, 4);
			bytes.Write(BitConverter.GetBytes(instance.g), 0, 4);
			bytes.Write(BitConverter.GetBytes(instance.b), 0, 4);
		}

		public static Color Deserialize(Stream bytes) {
			byte[] array = new byte[12];
			bytes.Read(array, 0, 12);
			return new Color(BitConverter.ToSingle(array, 0), BitConverter.ToSingle(array, 4), BitConverter.ToSingle(array, 8));
		}
	}
}
