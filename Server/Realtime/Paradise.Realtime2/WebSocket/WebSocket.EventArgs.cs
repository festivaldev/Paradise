using System;

namespace Paradise {
	public partial class WebSocket {
		public class SocketConnectedEventArgs : EventArgs {
			public WebSocketSharp.WebSocket Socket;
		}

		public class SocketDisconnectedEventArgs : EventArgs {
			public SocketInfo Info;
			public WebSocketSharp.WebSocket Socket;
			public uint Code;
			public string Reason;
		}

		public class SocketDataReceivedEventArgs : EventArgs {
			public WebSocketSharp.WebSocket Socket;
			public int BytesReceived;

			public Payload Payload;
			public object Data;

			public PacketType Type { get { return Payload.Type; } }
		}

		public class SocketDataSentEventArgs : EventArgs {
			public WebSocketSharp.WebSocket Socket;
			public int BytesSent;
		}

		public class SocketConnectionRejectedEventArgs : EventArgs {
			public SocketInfo Info;
			public WebSocketSharp.WebSocket Socket;
			public string Reason;
		}

		public class SocketStateChangedEventArgs : EventArgs {
			public WebSocketSharp.WebSocket Socket;
			//public SocketState State;
		}
	}
}
