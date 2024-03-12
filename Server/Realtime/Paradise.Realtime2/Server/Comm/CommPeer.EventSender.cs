using log4net;
using Photon.SocketServer;
using System;
using System.IO;
using UberStrike.Core.Serialization;
using UberStrike.Core.ViewModel;
using UberStrike.Realtime.Client;

namespace Paradise.Realtime.Server.Comm {
	public partial class CommPeer {
		public class EventSender : BaseEventSender {
			protected static readonly ILog Log = LogManager.GetLogger(nameof(CommPeer.EventSender));

			public LobbyRoom.EventSender LobbyEventSender { get; private set; }

			public EventSender(BasePeer peer) : base(peer) {
				LobbyEventSender = new LobbyRoom.EventSender(peer);
			}

			public override SendResult SendEvent(byte opCode, MemoryStream bytes) {
				if (Enum.IsDefined(typeof(ICommPeerEventsType), (int)opCode)) {
					Log.Debug($"CommPeer.EventSender::SendEvent: {(ICommPeerEventsType)opCode}");
				} else {
					Log.Debug($"CommPeer.EventSender::SendEvent: Unknown OpCode {opCode}");
				}

				return base.SendEvent(opCode, bytes);
			}

			#region Implementation of ICommPeerEventsType
			/// <summary>
			/// Sends the heartbeat challenge to the client.
			/// </summary>
			public void SendHeartbeatChallenge(string challengeHash) {
				using (var bytes = new MemoryStream()) {
					StringProxy.Serialize(bytes, challengeHash);

					SendEvent((byte)ICommPeerEventsType.HeartbeatChallenge, bytes);
				}
			}

			/// <summary>
			/// Sends the current server load (players connected, capacity) to the client
			/// </summary>
			/// <param name="data"></param>
			public void SendLoadData(ServerConnectionView data) {
				using (var bytes = new MemoryStream()) {
					ServerConnectionViewProxy.Serialize(bytes, data);

					SendEvent((byte)ICommPeerEventsType.LoadData, bytes);
				}
			}

			/// <summary>
			/// Sends an event about a successful lobby connection to the client
			/// </summary>
			public void SendLobbyEntered() {
				using (var bytes = new MemoryStream()) {
					SendEvent((byte)ICommPeerEventsType.LobbyEntered, bytes);
				}
			}

			/// <summary>
			/// Sends an error message to the clients and makes the game quit.
			/// </summary>
			public void SendDisconnectAndDisablePhoton(string message) {
				using (var bytes = new MemoryStream()) {
					StringProxy.Serialize(bytes, message);

					SendEvent((byte)ICommPeerEventsType.DisconnectAndDisablePhoton, bytes);
				}
			}
			#endregion
		}
	}
}
