using UnityEngine;

namespace Paradise.Core.Models {
	public struct ShortVector3 {
		public ShortVector3(Vector3 value) {
			this.value = value;
		}

		public float x {
			get {
				return this.value.x;
			}
		}

		public float y {
			get {
				return this.value.y;
			}
		}

		public float z {
			get {
				return this.value.z;
			}
		}

		public static implicit operator Vector3(ShortVector3 value) {
			return value.value;
		}

		public static implicit operator ShortVector3(Vector3 value) {
			return new ShortVector3(value);
		}

		public static ShortVector3 operator *(ShortVector3 vector, float value) {
			vector.value.x = vector.value.x * value;
			vector.value.y = vector.value.y * value;
			vector.value.z = vector.value.z * value;
			return vector;
		}

		public static ShortVector3 operator +(ShortVector3 vector, ShortVector3 value) {
			vector.value.x = vector.value.x + value.value.x;
			vector.value.y = vector.value.y + value.value.y;
			vector.value.z = vector.value.z + value.value.z;
			return vector;
		}

		public static ShortVector3 operator -(ShortVector3 vector, ShortVector3 value) {
			vector.value.x = vector.value.x - value.value.x;
			vector.value.y = vector.value.y - value.value.y;
			vector.value.z = vector.value.z - value.value.z;
			return vector;
		}

		private Vector3 value;
	}
}
