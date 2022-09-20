using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Paradise.Realtime.Server.Comm {
	public abstract class BaseLobbyRoomOperationHandler : BaseOperationHandler<CommPeer> {
		private static readonly ILog Log = LogManager.GetLogger(nameof(BaseLobbyRoomOperationHandler));

		protected object _lock { get; } = new object();

		public override int Id => 0;

		protected abstract void OnFullPlayerListUpdate(CommPeer peer);
		protected abstract void OnUpdatePlayerRoom(CommPeer peer, GameRoom room);
		protected abstract void OnResetPlayerRoom(CommPeer peer);
		protected abstract void OnUpdateFriendsList(CommPeer peer, int cmid);
		protected abstract void OnUpdateClanData(CommPeer peer, int cmid);
		protected abstract void OnUpdateInboxMessages(CommPeer peer, int cmid, int messageId);
		protected abstract void OnUpdateInboxRequests(CommPeer peer, int cmid);
		protected abstract void OnUpdateClanMembers(CommPeer peer, List<int> clanMembers);
		protected abstract void OnGetPlayersWithMatchingName(CommPeer peer, string search);
		protected abstract void OnChatMessageToAll(CommPeer peer, string message);
		protected abstract void OnChatMessageToPlayer(CommPeer peer, int cmid, string message);
		protected abstract void OnChatMessageToClan(CommPeer peer, List<int> clanMembers, string message);
		protected abstract void OnModerationMutePlayer(CommPeer peer, int durationInMinutes, int mutedCmid, bool disableChat);
		protected abstract void OnModerationPermanentBan(CommPeer peer, int cmid);
		protected abstract void OnModerationBanPlayer(CommPeer peer, int cmid);
		protected abstract void OnModerationKickGame(CommPeer peer, int cmid);
		protected abstract void OnModerationUnbanPlayer(CommPeer peer, int cmid);
		protected abstract void OnModerationCustomMessage(CommPeer peer, int cmid, string message);
		protected abstract void OnSpeedhackDetection(CommPeer peer);
		protected abstract void OnSpeedhackDetectionNew(CommPeer peer, List<float> timeDifferences);
		protected abstract void OnPlayersReported(CommPeer peer, List<int> cmids, int type, string details, string logs);
		protected abstract void OnUpdateNaughtyList(CommPeer peer);
		protected abstract void OnClearModeratorFlags(CommPeer peer, int cmid);
		protected abstract void OnSetContactList(CommPeer peer, List<int> cmids);
		protected abstract void OnUpdateAllActors(CommPeer peer);
		protected abstract void OnUpdateContacts(CommPeer peer);

		public override void OnOperationRequest(CommPeer peer, byte opCode, MemoryStream bytes) {
			Log.Debug($"LobbyRoom.OnOperationRequest peer: {peer}, opCode: {(ILobbyRoomOperationsType)opCode}({opCode})");

			var operation = (ILobbyRoomOperationsType)(opCode);
			switch (operation) {
				case ILobbyRoomOperationsType.FullPlayerListUpdate: {
					OnFullPlayerListUpdate(peer);
					break;
				}
				case ILobbyRoomOperationsType.UpdatePlayerRoom: {
					var room = GameRoomProxy.Deserialize(bytes);

					OnUpdatePlayerRoom(peer, room);
					break;
				}
				case ILobbyRoomOperationsType.ResetPlayerRoom: {
					OnResetPlayerRoom(peer);
					break;
				}
				case ILobbyRoomOperationsType.UpdateFriendsList: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnUpdateFriendsList(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.UpdateClanData: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnUpdateClanData(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.UpdateInboxMessages: {
					var cmid = Int32Proxy.Deserialize(bytes);
					var messageId = Int32Proxy.Deserialize(bytes);

					OnUpdateInboxMessages(peer, cmid, messageId);
					break;
				}
				case ILobbyRoomOperationsType.UpdateInboxRequests: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnUpdateInboxRequests(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.UpdateClanMembers: {
					var clanMembers = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);

					OnUpdateClanMembers(peer, clanMembers);
					break;
				}
				case ILobbyRoomOperationsType.GetPlayersWithMatchingName: {
					var search = StringProxy.Deserialize(bytes);

					OnGetPlayersWithMatchingName(peer, search);
					break;
				}
				case ILobbyRoomOperationsType.ChatMessageToAll: {
					var message = StringProxy.Deserialize(bytes);

					OnChatMessageToAll(peer, message);
					break;
				}
				case ILobbyRoomOperationsType.ChatMessageToPlayer: {
					var cmid = Int32Proxy.Deserialize(bytes);
					var message = StringProxy.Deserialize(bytes);

					OnChatMessageToPlayer(peer, cmid, message);
					break;
				}
				case ILobbyRoomOperationsType.ChatMessageToClan: {
					var clanMembers = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);
					var message = StringProxy.Deserialize(bytes);

					OnChatMessageToClan(peer, clanMembers, message);
					break;
				}
				case ILobbyRoomOperationsType.ModerationMutePlayer: {
					var durationInMinutes = Int32Proxy.Deserialize(bytes);
					var mutedCmid = Int32Proxy.Deserialize(bytes);
					var disableChat = BooleanProxy.Deserialize(bytes);

					OnModerationMutePlayer(peer, durationInMinutes, mutedCmid, disableChat);
					break;
				}
				case ILobbyRoomOperationsType.ModerationPermanentBan: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnModerationPermanentBan(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.ModerationBanPlayer: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnModerationBanPlayer(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.ModerationKickGame: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnModerationKickGame(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.ModerationUnbanPlayer: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnModerationUnbanPlayer(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.ModerationCustomMessage: {
					var cmid = Int32Proxy.Deserialize(bytes);
					var message = StringProxy.Deserialize(bytes);

					OnModerationCustomMessage(peer, cmid, message);
					break;
				}
				case ILobbyRoomOperationsType.SpeedhackDetection: {
					OnSpeedhackDetection(peer);
					break;
				}
				case ILobbyRoomOperationsType.SpeedhackDetectionNew: {
					var timeDifferences = ListProxy<float>.Deserialize(bytes, SingleProxy.Deserialize);

					OnSpeedhackDetectionNew(peer, timeDifferences);
					break;
				}
				case ILobbyRoomOperationsType.PlayersReported: {
					var cmids = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);
					var type = Int32Proxy.Deserialize(bytes);
					var details = StringProxy.Deserialize(bytes);
					var logs = StringProxy.Deserialize(bytes);

					OnPlayersReported(peer, cmids, type, details, logs);
					break;
				}
				case ILobbyRoomOperationsType.UpdateNaughtyList: {
					OnUpdateNaughtyList(peer);
					break;
				}
				case ILobbyRoomOperationsType.ClearModeratorFlags: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnClearModeratorFlags(peer, cmid);
					break;
				}
				case ILobbyRoomOperationsType.SetContactList: {
					var cmids = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);

					OnSetContactList(peer, cmids);
					break;
				}
				case ILobbyRoomOperationsType.UpdateAllActors: {
					OnUpdateAllActors(peer);
					break;
				}
				case ILobbyRoomOperationsType.UpdateContacts: {
					OnUpdateContacts(peer);
					break;
				}
				default:
					//throw new NotSupportedException();
					break;
			}
		}

		protected CommPeer FindPeerWithCmid(int cmid) {
			lock (_lock) {
				foreach (var peer in LobbyManager.Instance.Peers) {
					if (peer.Actor.Cmid == cmid) {
						return peer;
					}
				}
			}

			return null;
		}

		protected bool IsSpeedHacking(List<float> timeDifferences) {
			float mean = 0;
			for (int i = 0; i < timeDifferences.Count; i++)
				mean += timeDifferences[i];

			mean /= timeDifferences.Count;
			if (mean > 2f)
				return true;

			float variance = 0;
			for (int i = 0; i < timeDifferences.Count; i++)
				variance += (float)Math.Pow(timeDifferences[i] - mean, 2);

			variance /= timeDifferences.Count - 1;
			return mean > 1.1f && variance <= 0.05f;
		}


		private void DebugOperation(params object[] data) {
			Log.Debug($"{GetType().Name}:{new StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
		}
	}
}
