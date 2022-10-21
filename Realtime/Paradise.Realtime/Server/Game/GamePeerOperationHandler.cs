using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class GamePeerOperationHandler : BaseGamePeerOperationHandler {
		private enum GAME_FLAGS {
			None = 0,
			LowGravity = 1,
			NoArmor = 2,
			QuickSwitch = 4,
			MeleeOnly = 8
		}

		private static readonly ILog Log = LogManager.GetLogger("GameLog");

		

		protected override void OnSendHeartbeatResponse(GamePeer peer, string authToken, string responseHash) {
			try {
				if (!peer.CheckHeartbeat(responseHash)) {
					peer.Disconnect();
				}
			} catch {
				peer.SendError();
				throw;
			}
		}

		protected override void OnGetServerLoad(GamePeer peer) {
			peer.PeerEvents.SendServerLoadData(new PhotonServerLoad {
				MaxPlayerCount = 100,
				PeersConnected = GameApplication.Instance.Peers,
				RoomsCreated = GameApplication.Instance.RoomManager.Rooms.Count,
				TimeStamp = DateTime.UtcNow,
				State = PhotonServerLoad.Status.Alive
			});
		}

		protected override void OnGetGameInformation(GamePeer peer, int number) {
			throw new NotImplementedException();
		}

		protected override void OnGetGameListUpdates(GamePeer peer) {
			var rooms = new List<GameRoomData>();

			rooms.AddRange(GameApplication.Instance.RoomManager.Rooms.Select(_ => _.Value.MetaData));

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

			peer.PeerEvents.SendFullGameList(rooms);
		}

		protected override void OnEnterRoom(GamePeer peer, int roomId, string password, string clientVersion, string authToken, string magicHash) {
			if (clientVersion != "4.7.1") {
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, -1, $"Unsupported client version {clientVersion}, expected 4.7.1");
				return;
			}

			peer.Authenticate(authToken, magicHash);
			peer.Member = UserWebServiceClient.Instance.GetMember(peer.AuthToken);
			peer.Loadout = UserWebServiceClient.Instance.GetLoadout(peer.AuthToken);

			var room = GameApplication.Instance.RoomManager.GetRoom(roomId);
			if (room != null) {
				if (room.MetaData.IsPasswordProtected && password != room.Password) {
					peer.PeerEvents.SendRequestPasswordForRoom(room.MetaData.Server.ConnectionString, room.RoomId);
				} else {
					room.Join(peer);
				}
			} else {
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, roomId, "Game does not exist anymore.");
			}
		}

		protected override void OnCreateRoom(GamePeer peer, GameRoomData metaData, string password, string clientVersion, string authToken, string magicHash) {
			if (clientVersion != "4.7.1") {
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, -1, $"Unsupported client version {clientVersion}, expected 4.7.1");
				return;
			}

			peer.Authenticate(authToken, magicHash);
			peer.Member = UserWebServiceClient.Instance.GetMember(peer.AuthToken);

			var room = default(BaseGameRoom);
			try {
				room = GameApplication.Instance.RoomManager.CreateRoom(metaData, password);
			} catch (NotSupportedException e) {
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: Unsupported game mode.");
				return;
			} catch (Exception e) {
				Log.Error($"{peer.Actor.PlayerName}({peer.Actor.Cmid}): Failed to create game room: Unsupported game mode", e);

#if !DEBUG
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, -1, $"Failed to create game room.");
#else
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, -1, $"Failed to create game room.\n{e.Message}");
#endif
				return;
			}

			try {
				room.Join(peer);
			} catch (Exception e) {
				Log.Error($"{peer.Actor.PlayerName}({peer.Actor.Cmid}): Failed to join game room.", e);

#if !DEBUG
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, -1, $"Failed to join game room.");
#else
				peer.PeerEvents.SendRoomEnterFailed(string.Empty, -1, $"Failed to join game room.\n{e.Message}");
#endif
				GameApplication.Instance.RoomManager.RemoveRoom(room.RoomId);
			}
		}

		protected override void OnLeaveRoom(GamePeer peer) {
			var room = peer.Room;

			if (room != null) {
				room.Leave(peer);

				if (room.Peers.Count <= 0) {
					GameApplication.Instance.RoomManager.RemoveRoom(room.RoomId);
				}
			} else {
				Log.Error("A client tried to leave a room without being in a room.");
			}
		}

		protected override void OnCloseRoom(GamePeer peer, int roomId, string authToken, string magicHash) {
			throw new NotImplementedException();
		}

		protected override void OnInspectRoom(GamePeer peer, int roomId, string authToken) {
			throw new NotImplementedException();
		}

		protected override void OnReportPlayer(GamePeer peer, int cmid, string authToken) {
			throw new NotImplementedException();
		}

		protected override void OnKickPlayer(GamePeer peer, int cmid, string authToken, string magicHash) {
			var targetPeer = GameApplication.Instance.RoomManager.FindPeerWithCmid(cmid);

			if (targetPeer != null) {
				targetPeer.PeerEvents.SendRoomEnterFailed(string.Empty, 0, "You have been removed from the game.");
			}
		}

		protected override void OnUpdateLoadout(GamePeer peer) {
			var loadout = UserWebServiceClient.Instance.GetLoadout(peer.AuthToken);

			peer.Loadout = loadout;

			peer.Actor.Info.Weapons = new List<int> {
				loadout.MeleeWeapon,
				loadout.Weapon1,
				loadout.Weapon2,
				loadout.Weapon3
			};

			peer.Actor.Info.Gear = new List<int> {
				(int)loadout.Webbing,
				loadout.Head,
				loadout.Face,
				loadout.Gloves,
				loadout.UpperBody,
				loadout.LowerBody,
				loadout.Boots
			};
		}

		protected override void OnUpdatePing(GamePeer peer, ushort ping) {
			peer.Actor.Info.Ping = (ushort)(peer.RoundTripTime / 2);
		}

		protected override void OnUpdateKeyState(GamePeer peer, byte state) {
			peer.Actor.Movement.KeyState = state;
		}

		protected override void OnRefreshBackendData(GamePeer peer, string authToken, string magicHash) {
			throw new NotSupportedException();
		}

	}
}
