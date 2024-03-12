using System;

namespace Paradise {
	public partial class WebSocket {
		public enum ServerType {
			None,
			MasterServer,
			Comm,
			Game
		}

		public struct SocketInfo {
			public Guid SocketId;
			public ServerType Type;
			public bool IsClient;
		}

		public struct SocketConnectionStatus {
			public bool Connected;
			public bool Rejected;
			public string DisconnectReason;
		}

		public struct SocketChatMessage {
			public int Cmid;
			public string Name;
			public string Message;
		}
	}
}
