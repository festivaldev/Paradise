using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class GamePeerOperationsHandler : BaseGamePeerOperationsHandler {
		private static readonly ILog Log = LogManager.GetLogger(typeof(GamePeerOperationsHandler));

		protected override void OnCloseRoom(GamePeer peer, int roomId, string authToken, string magicHash) {
			throw new NotImplementedException();
		}

		protected override void OnCreateRoom(GamePeer peer, GameRoomData metaData, string password, string clientVersion, string authToken, string magicHash) {
			if (clientVersion != "4.7.1") {
				peer.Events.SendRoomEnterFailed(string.Empty, -1, $"Unsupported client version {clientVersion}, expected 4.7.1");
				return;
			}

			var userWebServiceClient = new UserWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl);

			peer.Authenticate(authToken, magicHash);
			peer.Member = userWebServiceClient.GetMember(authToken);

			var room = default(BaseGameRoom);
			try {
				room = GameApplication.Instance.RoomManager.CreateRoom(metaData, password);
			} catch (NotSupportedException e) {
				peer.Events.SendRoomEnterFailed(string.Empty, -1, "There was an error creating the game room: Unsupported game mode.");

				return;
			} catch (Exception e) {
#if !DEBUG
				peer.Events.SendRoomEnterFailed(string.Empty, -1, $"Failed to create game room.");
#else
				peer.Events.SendRoomEnterFailed(string.Empty, -1, $"Failed to create game room.\n{e.Message}");
#endif
				return;
			}

			room.Join(peer);
		}

		protected override void OnEnterRoom(GamePeer peer, int roomId, string password, string clientVersion, string authToken, string magicHash) {
			if (clientVersion != "4.7.1") {
				peer.Events.SendRoomEnterFailed(string.Empty, -1, $"Unsupported client version {clientVersion}, expected 4.7.1");
				return;
			}

			var userWebServiceClient = new UserWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl);

			peer.Authenticate(authToken, magicHash);
			peer.Member = userWebServiceClient.GetMember(authToken);

			var room = GameApplication.Instance.RoomManager.GetRoom(roomId);
			if (room != null) {
				if (room.MetaData.IsPasswordProtected && password != room.Password) {
					peer.Events.SendRequestPasswordForRoom(room.MetaData.Server.ConnectionString, room.RoomId);
				} else {
					room.Join(peer);
				}
			} else {
				peer.Events.SendRoomEnterFailed(string.Empty, roomId, "Game does not exist anymore.");
			}
		}

		protected override void OnGetGameInformation(GamePeer peer, int number) {
			throw new NotImplementedException();
		}

		protected override void OnGetGameListUpdates(GamePeer peer) {
			var rooms = new List<GameRoomData>(GameApplication.Instance.RoomManager.Rooms.Count);

			foreach (var room in GameApplication.Instance.RoomManager.Rooms.Values) {
				rooms.Add(room.MetaData);
			}

			if (rooms.Count == 0) {
				rooms.Add(new GameRoomData {
					ConnectedPlayers = -1,
					PlayerLimit = -3,
					LevelMin = 127,
					LevelMax = 1,
					Name = "debug do not join",
					MapID = new Random((int)DateTime.UtcNow.Ticks).Next(3, 18),
					GameMode = GameModeType.TeamDeathMatch,
					IsPermanentGame = true,
					Server = new ConnectionAddress("127.0.0.1", 5056)
				});
			}

			peer.Events.SendFullGameList(rooms);
		}

		protected override void OnGetServerLoad(GamePeer peer) {
			peer.Events.SendServerLoadData(new PhotonServerLoad {
				MaxPlayerCount = 20,
				PeersConnected = GameApplication.Instance.Peers,
				RoomsCreated = GameApplication.Instance.RoomManager.Rooms.Count,
				TimeStamp = DateTime.UtcNow,
				State = PhotonServerLoad.Status.Alive
			});
		}

		protected override void OnInspectRoom(GamePeer peer, int roomId, string authToken) {
			throw new NotImplementedException();
		}

		protected override void OnKickPlayer(GamePeer peer, int cmid, string authToken, string magicHash) {
			throw new NotImplementedException();
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

		protected override void OnRefreshBackendData(GamePeer peer, string authToken, string magicHash) {
			throw new NotImplementedException();
		}

		protected override void OnReportPlayer(GamePeer peer, int cmid, string authToken) {
			throw new NotImplementedException();
		}

		protected override void OnSendHeartbeatResponse(GamePeer peer, string authToken, string responseHash) {
			throw new NotImplementedException();
		}

		protected override void OnUpdateKeyState(GamePeer peer, byte state) {
			peer.Actor.Movement.KeyState = state;
		}

		protected override void OnUpdateLoadout(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnUpdatePing(GamePeer peer, ushort ping) {
			peer.Actor.Info.Ping = (ushort)(peer.RoundTripTime / 2);
		}
	}
}
