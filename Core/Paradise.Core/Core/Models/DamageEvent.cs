using System;
using System.Collections.Generic;

namespace Paradise.Core.Models {
	[Serializable]
	public class DamageEvent {
		public Dictionary<byte, byte> Damage { get; set; }

		public byte BodyPartFlag { get; set; }

		public int DamageEffectFlag { get; set; }

		public float DamgeEffectValue { get; set; }

		public int Count {
			get {
				return (this.Damage == null) ? 0 : this.Damage.Count;
			}
		}

		public void Clear() {
			if (this.Damage == null) {
				this.Damage = new Dictionary<byte, byte>();
			}
			this.BodyPartFlag = 0;
			this.Damage.Clear();
		}

		public void AddDamage(byte angle, short damage, byte bodyPart, int damageEffectFlag, float damageEffectValue) {
			if (this.Damage == null) {
				this.Damage = new Dictionary<byte, byte>();
			}
			if (this.Damage.ContainsKey(angle)) {
				Dictionary<byte, byte> damage2;
				Dictionary<byte, byte> dictionary = damage2 = this.Damage;
				byte b = damage2[angle];
				dictionary[angle] = (byte)(b + (byte)damage);
			} else {
				this.Damage[angle] = (byte)damage;
			}
			this.BodyPartFlag |= bodyPart;
			this.DamageEffectFlag = damageEffectFlag;
			this.DamgeEffectValue = damageEffectValue;
		}
	}
}
