using Paradise.Core.Models;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class DamageEventProxy {
		public static void Serialize(Stream stream, DamageEvent instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				ByteProxy.Serialize(memoryStream, instance.BodyPartFlag);
				if (instance.Damage != null) {
					DictionaryProxy<byte, byte>.Serialize(memoryStream, instance.Damage, new DictionaryProxy<byte, byte>.Serializer<byte>(ByteProxy.Serialize), new DictionaryProxy<byte, byte>.Serializer<byte>(ByteProxy.Serialize));
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.DamageEffectFlag);
				SingleProxy.Serialize(memoryStream, instance.DamgeEffectValue);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static DamageEvent Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			DamageEvent damageEvent = new DamageEvent();
			damageEvent.BodyPartFlag = ByteProxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				damageEvent.Damage = DictionaryProxy<byte, byte>.Deserialize(bytes, new DictionaryProxy<byte, byte>.Deserializer<byte>(ByteProxy.Deserialize), new DictionaryProxy<byte, byte>.Deserializer<byte>(ByteProxy.Deserialize));
			}
			damageEvent.DamageEffectFlag = Int32Proxy.Deserialize(bytes);
			damageEvent.DamgeEffectValue = SingleProxy.Deserialize(bytes);
			return damageEvent;
		}
	}
}
