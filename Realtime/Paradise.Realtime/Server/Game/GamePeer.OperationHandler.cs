using Cmune.Core.Models;
using Cmune.DataCenter.Common.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UberStrike.Core.Models;
using UberStrike.Core.Serialization;

#if DEBUG
using UberStrike.Core.Types;
#endif

namespace Paradise.Realtime.Server.Game {
	public partial class GamePeer {
		public class OperationHandler : BaseOperationHandler<GamePeer, IGamePeerOperationsType> {
			private static readonly new ILog Log = LogManager.GetLogger(nameof(GamePeer.OperationHandler));

			public override int Id => (int)OperationHandlerId.GamePeer;

			public override void OnOperationRequest(GamePeer peer, byte opCode, MemoryStream bytes) {
				switch ((IGamePeerOperationsType)opCode) {
					case IGamePeerOperationsType.SendHeartbeatResponse:
					case IGamePeerOperationsType.UpdateKeyState:
						break;
					default:
						Log.Debug($"GamePeer.OperationHandler::OnOperationRequest -> peer: {peer}, opCode: {(IGamePeerOperationsType)opCode}({opCode})");
						break;
				}

				switch ((IGamePeerOperationsType)opCode) {
					case IGamePeerOperationsType.SendHeartbeatResponse:
						SendHeartbeatResponse(peer, bytes);
						break;

					case IGamePeerOperationsType.GetServerLoad:
						GetServerLoad(peer, bytes);
						break;

					case IGamePeerOperationsType.GetGameInformation:
						GetGameInformation(peer, bytes);
						break;

					case IGamePeerOperationsType.GetGameListUpdates:
						GetGameListUpdates(peer, bytes);
						break;

					case IGamePeerOperationsType.EnterRoom:
						EnterRoom(peer, bytes);
						break;

					case IGamePeerOperationsType.CreateRoom:
						CreateRoom(peer, bytes);
						break;

					case IGamePeerOperationsType.LeaveRoom:
						LeaveRoom(peer, bytes);
						break;

					case IGamePeerOperationsType.CloseRoom:
						CloseRoom(peer, bytes);
						break;

					case IGamePeerOperationsType.InspectRoom:
						InspectRoom(peer, bytes);
						break;

					case IGamePeerOperationsType.ReportPlayer:
						ReportPlayer(peer, bytes);
						break;

					case IGamePeerOperationsType.KickPlayer:
						KickPlayer(peer, bytes);
						break;

					case IGamePeerOperationsType.UpdateLoadout:
						UpdateLoadout(peer, bytes);
						break;

					case IGamePeerOperationsType.UpdatePing:
						UpdatePing(peer, bytes);
						break;

					case IGamePeerOperationsType.UpdateKeyState:
						UpdateKeyState(peer, bytes);
						break;

					case IGamePeerOperationsType.RefreshBackendData:
						RefreshBackendData(peer, bytes);
						break;

					default:
						throw new NotSupportedException();
				}
			}

			#region Implementation of IGamePeerOperationsType
			private void SendHeartbeatResponse(GamePeer peer, MemoryStream bytes) {
				var authToken = StringProxy.Deserialize(bytes);
				var responseHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, authToken, responseHash);

				try {
					if (!peer.CheckHeartbeat(responseHash)) {
						peer.SendError();
					}
				} catch (Exception e) {
					Log.Error("Exception while checking heartbeat", e);
					peer.SendError();
				}
			}

			private void GetServerLoad(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				peer.PeerEventSender.SendServerLoadData(new PhotonServerLoad {
					MaxPlayerCount = 100,
					PeersConnected = GameServerApplication.Instance.Peers,
					RoomsCreated = GameServerApplication.Instance.RoomManager.Rooms.Count,
					TimeStamp = DateTime.UtcNow,
					State = PhotonServerLoad.Status.Alive
				});
			}

			private void GetGameInformation(GamePeer peer, MemoryStream bytes) {
				var number = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, number);

				throw new NotImplementedException();
			}

			private void GetGameListUpdates(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				var rooms = GameServerApplication.Instance.RoomManager.Rooms.Select(_ => _.Value.MetaData).ToList<GameRoomData>();

#if DEBUG
				if (rooms.Count == 0) {
					rooms.Add(new GameRoomData {
						ConnectedPlayers = -1,
						PlayerLimit = -3,
						LevelMin = 127,
						LevelMax = 1,
						Name = "DEBUG GAME: DO NOT JOIN!",
						//MapID = new Random((int)DateTime.UtcNow.Ticks).Next(3, 18),
						MapID = 0,
						GameMode = GameModeType.TeamDeathMatch,
						IsPermanentGame = true,
						Server = new ConnectionAddress("127.0.0.1", 5056)
					});
				}
#endif

				peer.PeerEventSender.SendFullGameList(rooms);
			}

			private void EnterRoom(GamePeer peer, MemoryStream bytes) {
				var roomId = Int32Proxy.Deserialize(bytes);
				var password = StringProxy.Deserialize(bytes);
				var clientVersion = StringProxy.Deserialize(bytes);
				var authToken = StringProxy.Deserialize(bytes);
				var magicHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, roomId, password, clientVersion, authToken, magicHash);

				if (peer.Room != null) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, $"Cannot enter more than one room.");
					return;
				}

				if (clientVersion != "4.7.1") {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, $"Unsupported client version {clientVersion}, expected 4.7.1");
					return;
				}

				if (!peer.Authenticate(authToken, magicHash)) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: Invalid authentication.");
					return;
				}

				if (!(UserWebServiceClient.Instance.GetMember(authToken) is var member) || member.CmuneMemberView == null) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: An internal error occurred.");
					return;
				}

				peer.Member = member;

				if (!(UserWebServiceClient.Instance.GetLoadout(authToken) is var loadout) || loadout.Cmid == 0) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: An internal error occurred.");
					return;
				}

				peer.Loadout = loadout;

				if (GameServerApplication.Instance.RoomManager.TryGetRoom(roomId, out var room)) {
					if (room.MetaData.IsPasswordProtected && password != room.Password) {
						peer.PeerEventSender.SendRequestPasswordForRoom(room.MetaData.Server.ConnectionString, room.RoomId);
					} else {
						room.Join(peer);
					}
				} else {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, roomId, "Game does not exist anymore.");
				}

				var rooms = GameServerApplication.Instance.RoomManager.Rooms.Select(_ => _.Value.MetaData).ToList<GameRoomData>();
				peer.PeerEventSender.SendFullGameList(rooms);
			}

			private void CreateRoom(GamePeer peer, MemoryStream bytes) {
				var metaData = GameRoomDataProxy.Deserialize(bytes);
				var password = StringProxy.Deserialize(bytes);
				var clientVersion = StringProxy.Deserialize(bytes);
				var authToken = StringProxy.Deserialize(bytes);
				var magicHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, metaData, password, clientVersion, authToken, magicHash);

				if (peer.Room != null) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, $"Cannot create more than one room.");
					return;
				}

				if (clientVersion != "4.7.1") {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, $"Unsupported client version {clientVersion}, expected 4.7.1");
					return;
				}

				if (!peer.Authenticate(authToken, magicHash)) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: Invalid authentication.");
					return;
				}

				if (!(UserWebServiceClient.Instance.GetMember(authToken) is var member) || member.CmuneMemberView == null) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: An internal error occurred.");
					return;
				}

				peer.Member = member;

				var room = default(BaseGameRoom);
				try {
					room = GameServerApplication.Instance.RoomManager.CreateRoom(metaData, password);
				} catch (NotSupportedException) {
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: Unsupported game mode.");
					return;
				} catch (Exception e) {
					Log.Error($"{peer.Member.CmuneMemberView.PublicProfile.Name}({peer.Member.CmuneMemberView.PublicProfile.Cmid}): Failed to create game room.", e);

#if !DEBUG
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, $"Failed to create game room.");
#else
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, -1, $"Failed to create game room.\n{e.Message}");
#endif
					return;
				}

				try {
					room.Join(peer);
				} catch (Exception e) {
					Log.Error(e);
					Log.Error($"{peer.Member.CmuneMemberView.PublicProfile.Name}({peer.Member.CmuneMemberView.PublicProfile.Cmid}): Failed to join game room.", e);

#if !DEBUG
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, room.RoomId, $"Failed to join game room.");
#else
					peer.PeerEventSender.SendRoomEnterFailed(string.Empty, room.RoomId, $"Failed to join game room.\n{e.Message}");
#endif
					GameServerApplication.Instance.RoomManager.RemoveRoom(room.RoomId);
				}

				var rooms = GameServerApplication.Instance.RoomManager.Rooms.Select(_ => _.Value.MetaData).ToList<GameRoomData>();
				peer.PeerEventSender.SendFullGameList(rooms);
			}

			private void LeaveRoom(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				var room = peer.Room;

				if (room != null) {
					room.Leave(peer);

					if (room.Peers.Count <= 0) {
						GameServerApplication.Instance.RoomManager.RemoveRoom(room.RoomId);
					}
				} else {
					Log.Debug("A client tried to leave a room without being in a room.");
				}
			}

			private void CloseRoom(GamePeer peer, MemoryStream bytes) {
				var roomId = Int32Proxy.Deserialize(bytes);
				var authToken = StringProxy.Deserialize(bytes);
				var magicHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, roomId, authToken, magicHash);

				if (!peer.Authenticate(authToken, magicHash)) {
					return;
				}

				if (!(UserWebServiceClient.Instance.GetMember(authToken) is var member) || member.CmuneMemberView == null) {
					return;
				}

				if (member.CmuneMemberView.PublicProfile.AccessLevel < MemberAccessLevel.Admin) {
					return;
				}

				if (GameServerApplication.Instance.RoomManager.TryGetRoom(roomId, out var room)) {
					var peers = new List<GamePeer>(room.Peers);

					foreach (var roomPeer in peers) {
						room.Leave(roomPeer);
						roomPeer.PeerEventSender.SendRoomEnterFailed(string.Empty, 0, "Game has been closed.");
					}

					GameServerApplication.Instance.RoomManager.RemoveRoom(room.RoomId);

					var rooms = GameServerApplication.Instance.RoomManager.Rooms.Select(_ => _.Value.MetaData).ToList<GameRoomData>();
					peer.PeerEventSender.SendFullGameList(rooms);
				}
			}

			private void InspectRoom(GamePeer peer, MemoryStream bytes) {
				var roomId = Int32Proxy.Deserialize(bytes);
				var authToken = StringProxy.Deserialize(bytes);

				DebugOperation(peer, roomId, authToken);

				throw new NotSupportedException();
			}

			private void ReportPlayer(GamePeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);
				var authToken = StringProxy.Deserialize(bytes);

				DebugOperation(peer, cmid, authToken);

				throw new NotImplementedException();
			}

			private void KickPlayer(GamePeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);
				var authToken = StringProxy.Deserialize(bytes);
				var magicHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, cmid, authToken, magicHash);

				var targetPeer = GameServerApplication.Instance.RoomManager.FindPeerWithCmid(cmid);

				if (!peer.Authenticate(authToken, magicHash)) {
					return;
				}

				if (!(UserWebServiceClient.Instance.GetMember(authToken) is var member) || member.CmuneMemberView == null) {
					return;
				}

				if (member.CmuneMemberView.PublicProfile.AccessLevel < MemberAccessLevel.Moderator) {
					return;
				}

				targetPeer?.PeerEventSender.SendRoomEnterFailed(string.Empty, 0, "You have been removed from the game.");
			}

			private void UpdateLoadout(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				var loadout = UserWebServiceClient.Instance.GetLoadout(peer.AuthToken);

				peer.Loadout = loadout;

				if (peer.Actor != null) {
					peer.Actor.ActorInfo.Weapons = new List<int> {
						loadout.MeleeWeapon,
						loadout.Weapon1,
						loadout.Weapon2,
						loadout.Weapon3
					};

					peer.Actor.ActorInfo.Gear = new List<int> {
						(int)loadout.Webbing,
						loadout.Head,
						loadout.Face,
						loadout.Gloves,
						loadout.UpperBody,
						loadout.LowerBody,
						loadout.Boots
					};
				}
			}

			private void UpdatePing(GamePeer peer, MemoryStream bytes) {
				var ping = UInt16Proxy.Deserialize(bytes);

				DebugOperation(peer, ping);

				if (peer.Actor != null) {
					peer.Actor.ActorInfo.Ping = (ushort)(peer.RoundTripTime / 2);
				}
			}

			private void UpdateKeyState(GamePeer peer, MemoryStream bytes) {
				var state = ByteProxy.Deserialize(bytes);

				DebugOperation(peer, state);

				if (peer.Actor != null) {
					peer.Actor.Movement.KeyState = state;
				}
			}

			private void RefreshBackendData(GamePeer peer, MemoryStream bytes) {
				var authToken = StringProxy.Deserialize(bytes);
				var magicHash = StringProxy.Deserialize(bytes);

				DebugOperation(peer, authToken, magicHash);

				throw new NotSupportedException();
			}
			#endregion



			private void DebugOperation(params object[] data) {
#if DEBUG
				Log.Debug($"{GetType().Name}:{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
#endif
			}
		}
	}
}
