using Paradise.Core.Models;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class GameRoomProxy {
		public static void Serialize(Stream stream, GameRoom instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.MapId);
				Int32Proxy.Serialize(memoryStream, instance.Number);
				if (instance.Server != null) {
					ConnectionAddressProxy.Serialize(memoryStream, instance.Server);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static GameRoom Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			GameRoom gameRoom = new GameRoom();
			gameRoom.MapId = Int32Proxy.Deserialize(bytes);
			gameRoom.Number = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				gameRoom.Server = ConnectionAddressProxy.Deserialize(bytes);
			}
			return gameRoom;
		}
	}
}
