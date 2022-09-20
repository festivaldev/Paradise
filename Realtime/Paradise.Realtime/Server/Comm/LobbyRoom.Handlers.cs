using log4net;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Comm {
	public partial class LobbyRoom : BaseLobbyRoomOperationHandler {
		private static ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		public override void OnDisconnect(CommPeer peer, DisconnectReason reasonCode, string reasonDetail) {
			Log.Debug($"{peer.Actor.Cmid} Disconnected {reasonCode} -> {reasonDetail}");

			// Remove the peer from the lobby list & update all peer's CommActor list.
			LobbyManager.Instance.GlobalLobby.Leave(peer);
			LobbyManager.Instance.UpdatePlayerList();
		}

		protected override void OnChatMessageToAll(CommPeer peer, string message) {
			if (peer.Actor.IsMuted) {
				return;
			}

			lock (_lock) {
				if (message.StartsWith("?") && peer.Actor.AccessLevel >= MemberAccessLevel.Moderator) {
					var args = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					string response = string.Empty;

					switch (args[0]) {
						case "?op":
							if (args.Length != 3) {
								response = "Usage: ?op <cmid> <level>";
							} else {
								if (!int.TryParse(args[1], out int cmid)) {
									response = "Error: <cmid> must be of type int.";
									break;
								}

								if (!int.TryParse(args[2], out int level)) {
									response = "Error: <level> must be of type int.";
									break;
								}

								if (cmid == peer.Actor.Cmid) {
									response = "Error: You cannot change your own permission level.";
									break;
								}

								if (!Enum.IsDefined(typeof(MemberAccessLevel), level)) {
									response = "Error: Invalid permission level.";
									break;
								}

								var result = ModerationWebServiceClient.Instance.OpPlayer(peer.AuthToken, cmid, (MemberAccessLevel)level);

								if (result != MemberOperationResult.Ok) {
									response = "Error: Could not set the selected player's permission level.\nMake sure the specified CMID is correct and that you are allowed to perform this action.";
								}
							}

							break;
						case "?deop":
							if (args.Length != 2) {
								response = "Usage: ?deop <cmid>";
							} else {
								if (!int.TryParse(args[1], out int cmid)) {
									response = "Error: <cmid> must be of type int.";
									break;
								}

								if (cmid == peer.Actor.Cmid) {
									response = "Error: You cannot change your own permission level.";
									break;
								}

								var result = ModerationWebServiceClient.Instance.DeopPlayer(peer.AuthToken, cmid);

								if (result != MemberOperationResult.Ok) {
									response = "Error: Could not set the selected player's permission level.\nMake sure the specified CMID is correct and that you are allowed to perform this action.";
								}
							}

							break;
						case "?ban":
							if (args.Length < 3) {
								response = "Usage: ?ban <cmid> <reason>";
							} else {
								if (!int.TryParse(args[1], out int cmid)) {
									response = "Error: <cmid> must be of type int.";
									break;
								}

								if (cmid == peer.Actor.Cmid) {
									response = "Error: You cannot ban yourself.";
									break;
								}

								var reason = string.Join(" ", args.Skip(2).Take(args.Count() - 2));
								var result = ModerationWebServiceClient.Instance.SetModerationFlag(peer.AuthToken, cmid, WebServices.ModerationFlag.Banned, DateTime.MaxValue, reason);

								if (result != MemberOperationResult.Ok) {
									response = "Error: Failed to ban player.";
								} else {
									FindPeerWithCmid(cmid)?.SendError($"You have been permanently banned.\nReason: {reason}");
								}
							}

							break;
						case "?unban":
							if (args.Length != 2) {
								response = "Usage: ?unban <cmid>";
							} else {
								if (!int.TryParse(args[1], out int cmid)) {
									response = "Error: <cmid> must be of type int.";
									break;
								}

								if (cmid == peer.Actor.Cmid) {
									response = "Error: You cannot unban yourself.";
									break;
								}

								var result = ModerationWebServiceClient.Instance.UnsetModerationFlag(peer.AuthToken, cmid, WebServices.ModerationFlag.Banned);

								if (result != MemberOperationResult.Ok) {
									response = "Error: Failed to unban player.";
								}
							}

							break;
						default:
							response = $"Unknown command \"{args[0]}\"";
							break;
					}

					if (!string.IsNullOrEmpty(response)) {
						peer.LobbyEvents.SendLobbyChatMessage(0, "[MOD]", response);
					}

					return;
				}
				foreach (var otherPeer in LobbyManager.Instance.GlobalLobby.Peers) {
					if (otherPeer.Actor.Cmid != peer.Actor.Cmid) {
						otherPeer.LobbyEvents.SendLobbyChatMessage(peer.Actor.Cmid, peer.Actor.Name, ProfanityFilter.CensorString(message));
					}
				}
			}
		}

		protected override void OnChatMessageToClan(CommPeer peer, List<int> clanMembers, string message) {
			if (peer.Actor.IsMuted) {
				return;
			}

			foreach (var cmid in clanMembers) {
				FindPeerWithCmid(cmid)?.LobbyEvents.SendClanChatMessage(peer.Actor.Cmid, peer.Actor.Name, ProfanityFilter.CensorString(message));
			}
		}

		protected override void OnChatMessageToPlayer(CommPeer peer, int cmid, string message) {
			if (peer.Actor.IsMuted) {
				return;
			}

			FindPeerWithCmid(cmid)?.LobbyEvents.SendPrivateChatMessage(peer.Actor.Cmid, peer.Actor.Name, ProfanityFilter.CensorString(message));
		}

		protected override void OnClearModeratorFlags(CommPeer peer, int cmid) {
			if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
				return;
			}

			ModerationWebServiceClient.Instance.ClearModerationFlags(peer.AuthToken, cmid);
		}

		protected override void OnFullPlayerListUpdate(CommPeer peer) {
			lock (_lock) {
				foreach (var otherPeer in LobbyManager.Instance.Peers) {
					if (otherPeer.Actor.Cmid != peer.Actor.Cmid) {
						peer.LobbyEvents.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
					}
				}
			}
		}

		protected override void OnGetPlayersWithMatchingName(CommPeer peer, string search) {
			// Appears to be unused
			throw new NotImplementedException();
		}

		protected override void OnModerationBanPlayer(CommPeer peer, int cmid) {
			if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
				return;
			}

			FindPeerWithCmid(cmid)?.SendError("You have been kicked from the game.");
		}

		protected override void OnModerationCustomMessage(CommPeer peer, int cmid, string message) {
			FindPeerWithCmid(cmid)?.LobbyEvents.SendModerationCustomMessage(message);
		}

		protected override void OnModerationKickGame(CommPeer peer, int cmid) {
			if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
				return;
			}

			FindPeerWithCmid(cmid)?.SendError("You have been kicked from the game.");
		}

		protected override void OnModerationMutePlayer(CommPeer peer, int durationInMinutes, int mutedCmid, bool disableChat) {
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
				mutedPeer.LobbyEvents.SendModerationMutePlayer(disableChat);
			}
		}

		protected override void OnModerationPermanentBan(CommPeer peer, int cmid) {
			if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
				return;
			}

			ModerationWebServiceClient.Instance.SetModerationFlag(peer.AuthToken, cmid, WebServices.ModerationFlag.Banned, DateTime.MaxValue, string.Empty);
			FindPeerWithCmid(cmid)?.SendError("You have been banned permanently.");
		}

		protected override void OnModerationUnbanPlayer(CommPeer peer, int cmid) {
			// Appears to be unused
			throw new NotImplementedException();
		}

		protected override void OnPlayersReported(CommPeer peer, List<int> cmids, int type, string details, string logs) {
			Log.Info("A player reported another player, but reporting isn't implemented yet.");
			//throw new NotImplementedException();
		}

		protected override void OnResetPlayerRoom(CommPeer peer) {
			try {
				peer.Actor.ActorInfo.CurrentRoom = null;

				lock (_lock) {
					foreach (var otherPeer in LobbyManager.Instance.Peers) {
						if (peer.Actor.Cmid != otherPeer.Actor.Cmid) {
							otherPeer.LobbyEvents.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
						}
					}
				}
			} catch (NullReferenceException e) {
				Log.Info("Trying to reset nonexistent player room. Did the server restart while players were ingame?");
				Log.Debug(e);
			}
		}

		protected override void OnSetContactList(CommPeer peer, List<int> cmids) {
			foreach (var cmid in cmids) {
				if (cmid == peer.Actor.Cmid) continue;

				FindPeerWithCmid(cmid)?.LobbyEvents.SendUpdateContacts(new List<CommActorInfo> { peer.Actor.ActorInfo }, new List<int> { });
			}
		}

		protected override void OnSpeedhackDetection(CommPeer peer) {
			peer.SendError();
		}

		protected override void OnSpeedhackDetectionNew(CommPeer peer, List<float> timeDifferences) {
			if (IsSpeedHacking(timeDifferences)) {
				peer.SendError();
			}
		}

		protected override void OnUpdateAllActors(CommPeer peer) {
			if (peer.Actor.AccessLevel >= MemberAccessLevel.Moderator) {
				peer.LobbyEvents.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
			}
		}

		protected override void OnUpdateClanData(CommPeer peer, int cmid) {
			FindPeerWithCmid(cmid)?.LobbyEvents.SendUpdateClanData();
		}

		protected override void OnUpdateClanMembers(CommPeer peer, List<int> clanMembers) {
			foreach (var cmid in clanMembers) {
				FindPeerWithCmid(cmid)?.LobbyEvents.SendUpdateClanMembers();
			}
		}

		protected override void OnUpdateContacts(CommPeer peer) {
			peer.LobbyEvents.SendUpdateContacts(new List<CommActorInfo>(), new List<int>());
		}

		protected override void OnUpdateFriendsList(CommPeer peer, int cmid) {
			FindPeerWithCmid(cmid)?.LobbyEvents.SendUpdateFriendsList();
		}

		protected override void OnUpdateInboxMessages(CommPeer peer, int cmid, int messageId) {
			FindPeerWithCmid(cmid)?.LobbyEvents.SendUpdateInboxMessages(messageId);
		}

		protected override void OnUpdateInboxRequests(CommPeer peer, int cmid) {
			FindPeerWithCmid(cmid)?.LobbyEvents.SendUpdateInboxRequests();
		}

		protected override void OnUpdateNaughtyList(CommPeer peer) {
			if (peer.Actor.AccessLevel < MemberAccessLevel.Moderator) {
				return;
			}

			peer.LobbyEvents.SendUpdateActorsForModeration(ModerationWebServiceClient.Instance.GetNaughtyList(peer.AuthToken));
		}

		protected override void OnUpdatePlayerRoom(CommPeer peer, GameRoom room) {
			peer.Actor.ActorInfo.CurrentRoom = room;

			foreach (var otherPeer in LobbyManager.Instance.Peers) {
				if (peer.Actor.Cmid != otherPeer.Actor.Cmid) {
					otherPeer.LobbyEvents.SendFullPlayerListUpdate(LobbyManager.Instance.Peers.Select(_ => _.Actor.ActorInfo).ToList());
				}
			}
		}
	}
}
