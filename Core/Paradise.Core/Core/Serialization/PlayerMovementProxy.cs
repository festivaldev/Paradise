using Paradise.Core.Models;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PlayerMovementProxy {
		public static void Serialize(Stream stream, PlayerMovement instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				ByteProxy.Serialize(memoryStream, instance.HorizontalRotation);
				ByteProxy.Serialize(memoryStream, instance.KeyState);
				ByteProxy.Serialize(memoryStream, instance.MovementState);
				ByteProxy.Serialize(memoryStream, instance.Number);
				ShortVector3Proxy.Serialize(memoryStream, instance.Position);
				ShortVector3Proxy.Serialize(memoryStream, instance.Velocity);
				ByteProxy.Serialize(memoryStream, instance.VerticalRotation);
				memoryStream.WriteTo(stream);
			}
		}

		public static PlayerMovement Deserialize(Stream bytes) {
			return new PlayerMovement {
				HorizontalRotation = ByteProxy.Deserialize(bytes),
				KeyState = ByteProxy.Deserialize(bytes),
				MovementState = ByteProxy.Deserialize(bytes),
				Number = ByteProxy.Deserialize(bytes),
				Position = ShortVector3Proxy.Deserialize(bytes),
				Velocity = ShortVector3Proxy.Deserialize(bytes),
				VerticalRotation = ByteProxy.Deserialize(bytes)
			};
		}
	}
}
