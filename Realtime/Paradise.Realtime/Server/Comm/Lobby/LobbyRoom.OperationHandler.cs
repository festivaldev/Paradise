using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Paradise.TcpSocket;

namespace Paradise.Realtime.Server.Comm {
	public partial class LobbyRoom {
		public class OperationHandler : BaseOperationHandler<CommPeer, ILobbyRoomOperationsType> {
			private static readonly new ILog Log = LogManager.GetLogger(nameof(LobbyRoom.OperationHandler));
			private static readonly ILog ChatLog = LogManager.GetLogger("ChatLog");

			public override int Id => (int)OperationHandlerId.LobbyRoom;

			protected object Lock { get; } = new object();
			private static readonly ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

			public override void OnOperationRequest(CommPeer peer, byte opCode, MemoryStream bytes) {
				Log.Debug($"LobbyRoom.OperationHandler::OnOperationRequest -> peer: {peer}, opCode: {(ILobbyRoomOperationsType)opCode}({opCode})");

				switch ((ILobbyRoomOperationsType)opCode) {
					case ILobbyRoomOperationsType.FullPlayerListUpdate:
						FullPlayerListUpdate(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdatePlayerRoom:
						UpdatePlayerRoom(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ResetPlayerRoom:
						ResetPlayerRoom(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateFriendsList:
						UpdateFriendsList(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateClanData:
						UpdateClanData(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateInboxMessages:
						UpdateInboxMessages(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateInboxRequests:
						UpdateInboxRequests(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateClanMembers:
						UpdateClanMembers(peer, bytes);
						break;

					case ILobbyRoomOperationsType.GetPlayersWithMatchingName:
						GetPlayersWithMatchingName(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ChatMessageToAll:
						ChatMessageToAll(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ChatMessageToPlayer:
						ChatMessageToPlayer(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ChatMessageToClan:
						ChatMessageToClan(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ModerationMutePlayer:
						ModerationMutePlayer(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ModerationPermanentBan:
						ModerationPermanentBan(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ModerationBanPlayer:
						ModerationBanPlayer(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ModerationKickGame:
						ModerationKickGame(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ModerationUnbanPlayer:
						ModerationUnbanPlayer(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ModerationCustomMessage:
						ModerationCustomMessage(peer, bytes);
						break;

					case ILobbyRoomOperationsType.SpeedhackDetection:
						SpeedhackDetection(peer, bytes);
						break;

					case ILobbyRoomOperationsType.SpeedhackDetectionNew:
						SpeedhackDetectionNew(peer, bytes);
						break;

					case ILobbyRoomOperationsType.PlayersReported:
						PlayersReported(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateNaughtyList:
						UpdateNaughtyList(peer, bytes);
						break;

					case ILobbyRoomOperationsType.ClearModeratorFlags:
						ClearModeratorFlags(peer, bytes);
						break;

					case ILobbyRoomOperationsType.SetContactList:
						SetContactList(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateAllActors:
						UpdateAllActors(peer, bytes);
						break;

					case ILobbyRoomOperationsType.UpdateContacts:
						UpdateContacts(peer, bytes);
						break;

					default:
						throw new NotSupportedException();
				}
			}

			public override void OnDisconnect(CommPeer peer, DisconnectReason reasonCode, string reasonDetail) {
				Log.Debug($"{peer.Actor.Cmid} Disconnected {reasonCode} -> {reasonDetail}");

				// Remove the peer from the lobby list & update all peer's CommActor list.
				LobbyManager.Instance.GlobalLobby.Leave(peer);
				LobbyManager.Instance.UpdatePlayerList();
			}

			#region Implementation of ILobbyRoomOperationsType
			private void FullPlayerListUpdate(CommPeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				lock (Lock) {
					foreach (var otherPeer in LobbyManager.Instance.Peers) {
						if (otherPeer.Actor.Cmid != peer.Actor.Cmid) {
							peer.LobbyEventSender.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
						}
					}
				}
			}

			private void UpdatePlayerRoom(CommPeer peer, MemoryStream bytes) {
				var room = GameRoomProxy.Deserialize(bytes);

				DebugOperation(peer, room);

				peer.Actor.ActorInfo.CurrentRoom = room;

				foreach (var otherPeer in LobbyManager.Instance.Peers) {
					if (peer.Actor.Cmid != otherPeer.Actor.Cmid) {
						otherPeer.LobbyEventSender.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
					}
				}
			}

			private void ResetPlayerRoom(CommPeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				try {
					peer.Actor.ActorInfo.CurrentRoom = null;

					lock (Lock) {
						foreach (var otherPeer in LobbyManager.Instance.Peers) {
							if (peer.Actor.Cmid != otherPeer.Actor.Cmid) {
								otherPeer.LobbyEventSender.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
							}
						}
					}
				} catch (NullReferenceException e) {
					Log.Info("Trying to reset nonexistent player room. Did the server restart while players were ingame?");
					Log.Debug(e);
				}
			}

			private void UpdateFriendsList(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				FindPeerWithCmid(cmid)?.LobbyEventSender.SendUpdateFriendsList();
			}

			private void UpdateClanData(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				FindPeerWithCmid(cmid)?.LobbyEventSender.SendUpdateClanData();
			}

			private void UpdateInboxMessages(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);
				var messageId = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid, messageId);

				FindPeerWithCmid(cmid)?.LobbyEventSender.SendUpdateInboxMessages(messageId);
			}

			private void UpdateInboxRequests(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				FindPeerWithCmid(cmid)?.LobbyEventSender.SendUpdateInboxRequests();
			}

			private void UpdateClanMembers(CommPeer peer, MemoryStream bytes) {
				var clanMembers = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);

				DebugOperation(peer, clanMembers);

				foreach (var cmid in clanMembers) {
					FindPeerWithCmid(cmid)?.LobbyEventSender.SendUpdateClanMembers();
				}
			}

			private void GetPlayersWithMatchingName(CommPeer peer, MemoryStream bytes) {
				var search = StringProxy.Deserialize(bytes);

				DebugOperation(peer, search);

				// Appears to be unused
				throw new NotImplementedException();
			}

			private void ChatMessageToAll(CommPeer peer, MemoryStream bytes) {
				var message = StringProxy.Deserialize(bytes);

				DebugOperation(peer, message);

				if (peer.Actor.IsMuted) {
					return;
				}

				lock (Lock) {
					if (message.StartsWith("?") && message.Length > 1) {
						var cmd = message.Substring(1);
						var cmdArgs = cmd.Split(' ').ToList();

						Task.Run(async () => {
							if (CommServerApplication.Instance.Socket == null) {
								peer.LobbyEventSender.SendLobbyChatMessage(0, "System", "Failed to execute command: Socket not connected");
							} else {
								var response = await CommServerApplication.Instance.Socket.Send(TcpSocket.PacketType.Command, new TcpSocket.SocketCommand {
									Command = cmdArgs.First(),
									Arguments = cmdArgs.Skip(1).Take(cmdArgs.Count - 1).ToArray(),
									Invoker = peer.Member.CmuneMemberView.PublicProfile
								}, false);

								if (!string.IsNullOrWhiteSpace((string)response)) {
									peer.LobbyEventSender.SendLobbyChatMessage(0, "System", (string)response);
								}
							}
						});

						return;
					}

					var censored = ProfanityFilter.CensorString(message);
					var trimmed = censored.Substring(0, Math.Min(censored.Length, 140));

					if (CommServerApplication.Instance.Configuration.EnableChatLog) {
						ChatLog.Info($"[Lobby] {peer.Actor.Name}: {trimmed}");
					}

					if (CommServerApplication.Instance.Configuration.DiscordChatIntegration) {
						CommServerApplication.Instance.Socket?.SendSync(PacketType.ChatMessage, new SocketChatMessage {
							Cmid = peer.Actor.Cmid,
							Name = peer.Actor.Name,
							Message = trimmed
						});
					}

					foreach (var otherPeer in LobbyManager.Instance.GlobalLobby.Peers) {
						if (otherPeer.Actor.Cmid != peer.Actor.Cmid) {
							otherPeer.LobbyEventSender.SendLobbyChatMessage(peer.Actor.Cmid, peer.Actor.Name, trimmed);
						}
					}
				}
			}

			private void ChatMessageToPlayer(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);
				var message = StringProxy.Deserialize(bytes);

				DebugOperation(peer, cmid, message);

				if (peer.Actor.IsMuted) {
					return;
				}

				var otherPeer = FindPeerWithCmid(cmid);

				if (otherPeer != null) {
					var censored = ProfanityFilter.CensorString(message);
					var trimmed = censored.Substring(0, Math.Min(censored.Length, 140));

					if (CommServerApplication.Instance.Configuration.EnableChatLog) {
						ChatLog.Info($"{peer.Actor.Name} → {otherPeer.Actor.Name}: {trimmed}");
					}

					otherPeer.LobbyEventSender.SendPrivateChatMessage(peer.Actor.Cmid, peer.Actor.Name, trimmed);
				}
			}

			private void ChatMessageToClan(CommPeer peer, MemoryStream bytes) {
				var clanMembers = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);
				var message = StringProxy.Deserialize(bytes);

				DebugOperation(peer, clanMembers, message);

				if (peer.Actor.IsMuted) {
					return;
				}

				var censored = ProfanityFilter.CensorString(message);
				var trimmed = censored.Substring(0, Math.Min(censored.Length, 140));

				if (CommServerApplication.Instance.Configuration.EnableChatLog) {
					ChatLog.Info($"{peer.Actor.Name} → Clan: {trimmed}");
				}

				foreach (var cmid in clanMembers) {
					FindPeerWithCmid(cmid)?.LobbyEventSender.SendClanChatMessage(peer.Actor.Cmid, peer.Actor.Name, trimmed);
				}
			}

			private void ModerationMutePlayer(CommPeer peer, MemoryStream bytes) {
				var durationInMinutes = Int32Proxy.Deserialize(bytes);
				var mutedCmid = Int32Proxy.Deserialize(bytes);
				var disableChat = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, durationInMinutes, mutedCmid, disableChat);

				if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
					return;
				}

				if (durationInMinutes > 0) {
					ModerationWebServiceClient.Instance.SetModerationFlag(peer.AuthToken, mutedCmid, disableChat ? WebServices.ModerationFlag.Muted : WebServices.ModerationFlag.Ghosted, DateTime.UtcNow.AddMinutes(durationInMinutes), string.Empty);
				} else {
					ModerationWebServiceClient.Instance.UnsetModerationFlag(peer.AuthToken, mutedCmid, disableChat ? WebServices.ModerationFlag.Muted : WebServices.ModerationFlag.Ghosted);
				}

				var mutedPeer = FindPeerWithCmid(mutedCmid);

				if (mutedPeer != null && mutedPeer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
					mutedPeer.Actor.MuteEndTime = DateTime.UtcNow.AddMinutes(durationInMinutes);
					mutedPeer.LobbyEventSender.SendModerationMutePlayer(disableChat);
				}
			}

			private void ModerationPermanentBan(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
					return;
				}

				ModerationWebServiceClient.Instance.SetModerationFlag(peer.AuthToken, cmid, WebServices.ModerationFlag.Banned, DateTime.MaxValue, string.Empty);
				FindPeerWithCmid(cmid)?.SendError("You have been banned permanently.");
			}

			private void ModerationBanPlayer(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
					return;
				}

				FindPeerWithCmid(cmid)?.SendError("You have been kicked from the game.");
			}

			private void ModerationKickGame(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
					return;
				}

				FindPeerWithCmid(cmid)?.SendError("You have been kicked from the game.");
			}

			private void ModerationUnbanPlayer(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				// Appears to be unused
				throw new NotImplementedException();
			}

			private void ModerationCustomMessage(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);
				var message = StringProxy.Deserialize(bytes);

				DebugOperation(peer, cmid, message);

				FindPeerWithCmid(cmid)?.LobbyEventSender.SendModerationCustomMessage(message);
			}

			private void SpeedhackDetection(CommPeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				peer.SendError();
			}

			private void SpeedhackDetectionNew(CommPeer peer, MemoryStream bytes) {
				var timeDifferences = ListProxy<float>.Deserialize(bytes, SingleProxy.Deserialize);

				DebugOperation(peer, timeDifferences);

				if (IsSpeedHacking(timeDifferences)) {
					peer.SendError();
				}
			}

			private void PlayersReported(CommPeer peer, MemoryStream bytes) {
				var cmids = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);
				var type = Int32Proxy.Deserialize(bytes);
				var details = StringProxy.Deserialize(bytes);
				var logs = StringProxy.Deserialize(bytes);

				DebugOperation(peer, cmids, type, details, logs);

				Log.Info("A player reported another player, but reporting isn't implemented yet.");
				//throw new NotImplementedException();
			}

			private void UpdateNaughtyList(CommPeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
					return;
				}

				peer.LobbyEventSender.SendUpdateActorsForModeration(ModerationWebServiceClient.Instance.GetNaughtyList(peer.AuthToken));
			}

			private void ClearModeratorFlags(CommPeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
					return;
				}

				ModerationWebServiceClient.Instance.ClearModerationFlags(peer.AuthToken, cmid);
			}

			private void SetContactList(CommPeer peer, MemoryStream bytes) {
				var cmids = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);

				DebugOperation(peer, cmids);

				foreach (var cmid in cmids) {
					if (cmid == peer.Actor.Cmid) continue;

					FindPeerWithCmid(cmid)?.LobbyEventSender.SendUpdateContacts(new List<CommActorInfo> { peer.Actor.ActorInfo }, new List<int> { });
				}
			}

			private void UpdateAllActors(CommPeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				if (peer.Actor.AccessLevel >= MemberAccessLevel.Moderator) {
					peer.LobbyEventSender.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
				}
			}

			private void UpdateContacts(CommPeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				peer.LobbyEventSender.SendUpdateContacts(new List<CommActorInfo>(), new List<int>());
			}
			#endregion



			protected CommPeer FindPeerWithCmid(int cmid) {
				lock (Lock) {
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
#if DEBUG
				Log.Debug($"{GetType().Name}:{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
#endif
			}
		}
	}
}
