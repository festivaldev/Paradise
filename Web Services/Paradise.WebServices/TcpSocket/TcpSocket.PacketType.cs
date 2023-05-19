namespace Paradise {
	public partial class TcpSocket {
		public enum PacketType {
			MagicBytes = 1,
			ClientInfo,
			ConnectionStatus,

			Ping,
			Pong,

			Command,
			CommandOutput,
			Monitoring,
			Error,

			ChatMessage = 1 << 10,
			//PlayerList,
			PlayerJoined,
			PlayerLeft,
			RoomOpened,
			RoomClosed,

			OpenRoom,
			CloseRoom,
			BanPlayer
		}
	}
}
