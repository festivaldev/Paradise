using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise {
	public partial class TcpSocket {
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

		public enum SocketState {
			Disconnected,
			Connecting,
			Connected,
			Sending,
			Receiving,
			Disconnecting
		}

		public struct SocketCommand {
			public string Command;
			public string[] Arguments;
			public PublicProfileView Invoker;
		}

		public struct SocketChatMessage {
			public int Cmid;
			public string Name;
			public string Message;
		}

		public struct RealtimeError {
			public ServerType Type;
			public Type ExceptionType;
			public string Message;
			public string StackTrace;
		}
	}
}
