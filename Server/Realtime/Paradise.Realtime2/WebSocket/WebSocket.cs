using Cmune.DataCenter.Common.Entities;
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

		public struct SocketCommand {
			public string Command;
			public string[] Arguments;
			public PublicProfileView Invoker;
		}

		public struct RealtimeError {
			public ServerType Type;
			public Type ExceptionType;
			public string Message;
			public string StackTrace;
		}
	}
}
