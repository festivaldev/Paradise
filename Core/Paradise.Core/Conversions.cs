using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise {
	public static class Conversion {
		public static T[] ToArray<T>(ICollection<T> collection) {
			T[] array = new T[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}

		public static Array ToArray(ICollection collection) {
			object[] array = new object[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}

		public static T ToEnum<T>(string value) {
			if (typeof(T).IsEnum && !string.IsNullOrEmpty(value) && Enum.IsDefined(typeof(T), value)) {
				return (T)((object)Enum.Parse(typeof(T), value));
			}
			return default(T);
		}

		public static float Deg2Rad(float angle) {
			return Mathf.Abs((angle % 360f + 360f) % 360f / 360f);
		}

		public static byte Angle2Byte(float angle) {
			return (byte)(255f * Conversion.Deg2Rad(angle));
		}

		public static float Byte2Angle(byte angle) {
			float num = 360f * (float)angle;
			return num / 255f;
		}

		public static ushort Angle2Short(float angle) {
			return (ushort)(65535f * Conversion.Deg2Rad(angle));
		}

		public static float Short2Angle(ushort angle) {
			float num = 360f * (float)angle;
			return num / 65535f;
		}
	}
}
