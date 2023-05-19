using System;

namespace Paradise {
	public partial class TcpSocket {
		public class SocketConnectedEventArgs : EventArgs {
			public SocketConnection Socket;
		}

		public class SocketDisconnectedEventArgs : EventArgs {
			public SocketInfo Info;
			public SocketConnection Socket;
			public string Reason;
		}

		public class SocketDataReceivedEventArgs : EventArgs {
			public SocketConnection Socket;
			public int BytesReceived;

			public Payload Payload;
			public object Data;

			public PacketType Type { get { return Payload.Type; } }
		}

		public class SocketDataSentEventArgs : EventArgs {
			public SocketConnection Socket;
			public int BytesSent;
		}

		public class SocketConnectionRejectedEventArgs : EventArgs {
			public SocketInfo Info;
			public SocketConnection Socket;
			public string Reason;
		}

		public class SocketStateChangedEventArgs : EventArgs {
			public SocketConnection Socket;
			public SocketState State;
		}
	}
}
