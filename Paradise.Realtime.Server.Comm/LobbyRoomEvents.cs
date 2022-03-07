using Paradise.Core.Models;
using Paradise.Core.Serialization;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime.Server.Comm {
	public class LobbyRoomEvents : EventSender {
		public LobbyRoomEvents(Peer peer) : base(peer) {
			// Space
		}

		public void SendUpdateFriendsList() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)ILobbyRoomEventsType.UpdateFriendsList, bytes);
			}
		}

		public void SendUpdateInboxMessages(int messageId) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, messageId);
				SendEvent((byte)ILobbyRoomEventsType.UpdateInboxMessages, bytes);
			}
		}

		public void SendPrivateChatMessage(int cmid, string name, string message) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				StringProxy.Serialize(bytes, name);
				StringProxy.Serialize(bytes, message);
				SendEvent((byte)ILobbyRoomEventsType.PrivateChatMessage, bytes);
			}
		}

		public void SendFullPlayerListUpdate(List<CommActorInfo> actors) {
			using (var bytes = new MemoryStream()) {
				ListProxy<CommActorInfo>.Serialize(bytes, actors, CommActorInfoProxy.Serialize);
				SendEvent((byte)ILobbyRoomEventsType.FullPlayerListUpdate, bytes);
			}
		}

		public void SendLobbyChatMessage(int cmid, string name, string message) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				StringProxy.Serialize(bytes, name);
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)ILobbyRoomEventsType.LobbyChatMessage, bytes);
			}
		}

		public void SendModerationCustomMessage(string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)ILobbyRoomEventsType.ModerationCustomMessage, bytes);
			}
		}

		public void SendModerationMutePlayer(bool isPlayerMuted) {
			using (var bytes = new MemoryStream()) {
				BooleanProxy.Serialize(bytes, isPlayerMuted);

				SendEvent((byte)ILobbyRoomEventsType.ModerationMutePlayer, bytes);
			}
		}

		public void SendModerationKickGame() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)ILobbyRoomEventsType.ModerationKickGame, bytes);
			}
		}
	}
}
