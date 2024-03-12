using Cmune.DataCenter.Common.Entities;
using log4net;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using UberStrike.Core.Models;
using UberStrike.Core.Serialization;
using UberStrike.Realtime.Client;

namespace Paradise.Realtime.Server.Comm {
	public partial class LobbyRoom {
		public class EventSender : BaseEventSender {
			private static readonly ILog Log = LogManager.GetLogger(nameof(LobbyRoom.EventSender));

			public EventSender(BasePeer peer) : base(peer) { }

			public override SendResult SendEvent(byte opCode, MemoryStream bytes) {
				if (Enum.IsDefined(typeof(ILobbyRoomEventsType), (int)opCode)) {
					Log.Debug($"LobbyRoom.EventSender::SendEvent: {(ILobbyRoomEventsType)opCode}");
				} else {
					Log.Error($"LobbyRoom.EventSender::SendEvent: Unknown OpCode {opCode}");
				}

				return base.SendEvent(opCode, bytes);
			}

			#region Implementation of ILobbyRoomEventsType
			public void SendPlayerHide(int cmid) {
				using (var bytes = new MemoryStream()) {
					Int32Proxy.Serialize(bytes, cmid);

					SendEvent((byte)ILobbyRoomEventsType.PlayerHide, bytes);
				}
			}

			public void SendPlayerLeft(int cmid, bool refreshComm) {
				using (var bytes = new MemoryStream()) {
					Int32Proxy.Serialize(bytes, cmid);
					BooleanProxy.Serialize(bytes, refreshComm);

					SendEvent((byte)ILobbyRoomEventsType.PlayerLeft, bytes);
				}
			}

			public void SendPlayerUpdate(CommActorInfo data) {
				using (var bytes = new MemoryStream()) {
					CommActorInfoProxy.Serialize(bytes, data);

					SendEvent((byte)ILobbyRoomEventsType.PlayerUpdate, bytes);
				}
			}

			public void SendUpdateContacts(List<CommActorInfo> updated, List<int> removed) {
				using (var bytes = new MemoryStream()) {
					ListProxy<CommActorInfo>.Serialize(bytes, updated, CommActorInfoProxy.Serialize);
					ListProxy<int>.Serialize(bytes, removed, Int32Proxy.Serialize);

					SendEvent((byte)ILobbyRoomEventsType.UpdateContacts, bytes);
				}
			}

			public void SendFullPlayerListUpdate(List<CommActorInfo> players) {
				using (var bytes = new MemoryStream()) {
					ListProxy<CommActorInfo>.Serialize(bytes, players, CommActorInfoProxy.Serialize);

					SendEvent((byte)ILobbyRoomEventsType.FullPlayerListUpdate, bytes);
				}
			}

			public void SendPlayerJoined(CommActorInfo data) {
				using (var bytes = new MemoryStream()) {
					CommActorInfoProxy.Serialize(bytes, data);

					SendEvent((byte)ILobbyRoomEventsType.PlayerJoined, bytes);
				}
			}

			public void SendClanChatMessage(int cmid, string name, string message) {
				using (var bytes = new MemoryStream()) {
					Int32Proxy.Serialize(bytes, cmid);
					StringProxy.Serialize(bytes, name);
					StringProxy.Serialize(bytes, message);

					SendEvent((byte)ILobbyRoomEventsType.ClanChatMessage, bytes);
				}
			}

			public void SendInGameChatMessage(int cmid, string name, string message, MemberAccessLevel accessLevel, byte context) {
				using (var bytes = new MemoryStream()) {
					Int32Proxy.Serialize(bytes, cmid);
					StringProxy.Serialize(bytes, name);
					StringProxy.Serialize(bytes, message);
					EnumProxy<MemberAccessLevel>.Serialize(bytes, accessLevel);
					ByteProxy.Serialize(bytes, context);

					SendEvent((byte)ILobbyRoomEventsType.InGameChatMessage, bytes);
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

			public void SendPrivateChatMessage(int cmid, string name, string message) {
				using (var bytes = new MemoryStream()) {
					Int32Proxy.Serialize(bytes, cmid);
					StringProxy.Serialize(bytes, name);
					StringProxy.Serialize(bytes, message);

					SendEvent((byte)ILobbyRoomEventsType.PrivateChatMessage, bytes);
				}
			}

			public void SendUpdateInboxRequests() {
				using (var bytes = new MemoryStream()) {
					SendEvent((byte)ILobbyRoomEventsType.UpdateInboxRequests, bytes);
				}
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

			public void SendUpdateClanMembers() {
				using (var bytes = new MemoryStream()) {
					SendEvent((byte)ILobbyRoomEventsType.UpdateClanMembers, bytes);
				}
			}

			public void SendUpdateClanData() {
				using (var bytes = new MemoryStream()) {
					SendEvent((byte)ILobbyRoomEventsType.UpdateClanData, bytes);
				}
			}

			public void SendUpdateActorsForModeration(List<CommActorInfo> allHackers) {
				using (var bytes = new MemoryStream()) {
					ListProxy<CommActorInfo>.Serialize(bytes, allHackers, CommActorInfoProxy.Serialize);

					SendEvent((byte)ILobbyRoomEventsType.UpdateActorsForModeration, bytes);
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
			#endregion
		}
	}
}
