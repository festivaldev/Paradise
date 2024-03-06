namespace Paradise {
	public partial class WebSocket {
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
			RoundStarted,
			RoundEnded,

			OpenRoom,
			CloseRoom,
			BanPlayer
		}
	}
}
