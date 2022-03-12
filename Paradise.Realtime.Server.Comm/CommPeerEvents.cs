using Paradise.Core.Serialization;
using Paradise.Core.ViewModel;
using System.IO;

namespace Paradise.Realtime.Server.Comm {
	public class CommPeerEvents : BaseEventSender {
		public LobbyRoomEvents Lobby { get; private set; }

		public CommPeerEvents(BasePeer peer) : base(peer) {
			Lobby = new LobbyRoomEvents(peer);
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
