using log4net;
using Paradise.Core.Serialization;
using Paradise.Core.ViewModel;
using Photon.SocketServer;
using System;
using System.IO;

namespace Paradise.Realtime.Server.Comm {
	public class CommPeerEvents : BaseEventSender {
		private readonly static ILog Log = LogManager.GetLogger(nameof(BaseEventSender));

		public LobbyRoomEvents LobbyEvents { get; private set; }

		public CommPeerEvents(BasePeer peer) : base(peer) {
			LobbyEvents = new LobbyRoomEvents(peer);
		}

		public override SendResult SendEvent(byte opCode, MemoryStream bytes) {
			if (Enum.IsDefined(typeof(ICommPeerEventsType), (int)opCode)) {
				Log.Debug($"CommPeerEvents.SendEvent: {(ICommPeerEventsType)opCode}");
			}

			return base.SendEvent(opCode, bytes);
		}

		public void SendHeartbeatChallenge(string challengeHash) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, challengeHash);

				SendEvent((byte)ICommPeerEventsType.HeartbeatChallenge, bytes);
			}
		}

		public void SendLoadData(ServerConnectionView data) {
			using (var bytes = new MemoryStream()) {
				ServerConnectionViewProxy.Serialize(bytes, data);

				SendEvent((byte)ICommPeerEventsType.LoadData, bytes);
			}
		}

		public void SendLobbyEntered() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)ICommPeerEventsType.LobbyEntered, bytes);
			}
		}

		public void SendDisconnectAndDisablePhoton(string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)ICommPeerEventsType.DisconnectAndDisablePhoton, bytes);
			}
		}
	}
}
