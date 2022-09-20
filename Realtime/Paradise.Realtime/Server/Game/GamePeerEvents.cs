using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime.Server.Game {
	public class GamePeerEvents : BaseEventSender {
		private readonly static ILog Log = LogManager.GetLogger(nameof(BaseEventSender));

		public GameRoomEvents GameEvents { get; private set; }

		public GamePeerEvents(BasePeer peer) : base(peer) {
			GameEvents = new GameRoomEvents(peer);
		}

		public override SendResult SendEvent(byte opCode, MemoryStream bytes) {
			if (Enum.IsDefined(typeof(IGamePeerEventsType), (int)opCode)) {
				switch ((IGamePeerEventsType)opCode) {
					case IGamePeerEventsType.HeartbeatChallenge:
						break;
					default:
						Log.Debug($"GameRoomEvents.SendEvent: {(IGamePeerEventsType)opCode}");
						break;
				}
			}

			return base.SendEvent(opCode, bytes);
		}

		/// <summary>
		/// Sends the heartbeat challenge to the client.
		/// </summary>
		public void SendHeartbeatChallenge(string challengeHash) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, challengeHash);

				SendEvent((byte)IGamePeerEventsType.HeartbeatChallenge, bytes);
			}
		}

		/// <summary>
		/// Sends information about the room (aka session) a client just entered to that client.
		/// </summary>
		public void SendRoomEntered(GameRoomData game) {
			using (var bytes = new MemoryStream()) {
				GameRoomDataProxy.Serialize(bytes, game);

				SendEvent((byte)IGamePeerEventsType.RoomEntered, bytes);
			}
		}

		/// <summary>
		/// Sends a message to the client why a room could not be entered.
		/// </summary>
		public void SendRoomEnterFailed(string server, int roomId, string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, server);
				Int32Proxy.Serialize(bytes, roomId);
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)IGamePeerEventsType.RoomEnterFailed, bytes);
			}
		}

		/// <summary>
		/// Sends a password request to the client. Will be called every time the client enters a wrong password.
		/// </summary>
		/// <remarks>
		/// Maybe add a counter + error to prevent a client from spamming?
		/// </remarks>
		public void SendRequestPasswordForRoom(string server, int roomId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, server);
				Int32Proxy.Serialize(bytes, roomId);

				SendEvent((byte)IGamePeerEventsType.RequestPasswordForRoom, bytes);
			}
		}

		/// <summary>
		/// Informs the client that they left a room.
		/// </summary>
		public void SendRoomLeft() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGamePeerEventsType.RoomLeft, bytes);
			}
		}

		/// <summary>
		/// Sends the full list of games to a client.
		/// </summary>
		public void SendFullGameList(List<GameRoomData> gameList) {
			using (var bytes = new MemoryStream()) {
				ListProxy<GameRoomData>.Serialize(bytes, gameList, GameRoomDataProxy.Serialize);

				SendEvent((byte)IGamePeerEventsType.FullGameList, bytes);
			}
		}

		/// <summary>
		/// Sends a list of updated (created, players changed, etc.) and removed games to the client.
		/// </summary>
		/// <remarks>
		/// Should probably be sent to clients periodically or automatically
		/// </remarks>
		public void SendGameListUpdate(List<GameRoomData> updatedGames, List<int> removedGames) {
			using (var bytes = new MemoryStream()) {
				ListProxy<GameRoomData>.Serialize(bytes, updatedGames, GameRoomDataProxy.Serialize);
				ListProxy<int>.Serialize(bytes, removedGames, Int32Proxy.Serialize);

				SendEvent((byte)IGamePeerEventsType.GameListUpdate, bytes);
			}
		}

		/// <summary>
		/// Clears the game list client-side
		/// </summary>
		public void SendGameListUpdateEnd() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGamePeerEventsType.GameListUpdateEnd, bytes);
			}
		}

		/// <summary>
		/// Unknown use. Probably sends game details to the client. Appears to be unused.
		/// </summary>
		public void SendGetGameInformation(GameRoomData room, List<GameActorInfo> players, int endTime) {
			using (var bytes = new MemoryStream()) {
				GameRoomDataProxy.Serialize(bytes, room);
				ListProxy<GameActorInfo>.Serialize(bytes, players, GameActorInfoProxy.Serialize);
				Int32Proxy.Serialize(bytes, endTime);

				SendEvent((byte)IGamePeerEventsType.GetGameInformation, bytes);
			}
		}

		/// <summary>
		/// Sends the current server load (players connected, capacity) to the client
		/// </summary>
		/// <param name="data"></param>
		public void SendServerLoadData(PhotonServerLoad data) {
			using (var bytes = new MemoryStream()) {
				PhotonServerLoadProxy.Serialize(bytes, data);

				SendEvent((byte)IGamePeerEventsType.ServerLoadData, bytes);
			}
		}

		/// <summary>
		/// Sends an error message to the clients and makes the game quit.
		/// </summary>
		public void SendDisconnectAndDisablePhoton(string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)IGamePeerEventsType.DisconnectAndDisablePhoton, bytes);
			}
		}
	}
}
