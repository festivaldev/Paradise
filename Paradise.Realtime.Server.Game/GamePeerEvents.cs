using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime.Server.Game {
	public class GamePeerEvents : BaseEventSender {
		private static readonly ILog Log = LogManager.GetLogger(typeof(GamePeerEvents));

		public GameRoomEvents Room { get; private set; }

		public GamePeerEvents(GamePeer peer) : base(peer) {
			Room = new GameRoomEvents(peer);
		}

		public void SendHeartbeatChallenge(string challengeHash) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, challengeHash);

				SendEvent((byte)IGamePeerEventsType.HeartbeatChallenge, bytes);
			}
		}

		public void SendRoomEntered(GameRoomData game) {
			using (var bytes = new MemoryStream()) {
				GameRoomDataProxy.Serialize(bytes, game);

				SendEvent((byte)IGamePeerEventsType.RoomEntered, bytes);
			}
		}

		public void SendRoomEnterFailed(string server, int roomId, string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, server);
				Int32Proxy.Serialize(bytes, roomId);
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)IGamePeerEventsType.RoomEnterFailed, bytes);
			}
		}

		public void SendRequestPasswordForRoom(string server, int roomId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, server);
				Int32Proxy.Serialize(bytes, roomId);

				SendEvent((byte)IGamePeerEventsType.RequestPasswordForRoom, bytes);
			}
		}

		public void SendRoomLeft() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGamePeerEventsType.RoomLeft, bytes);
			}
		}

		public void SendFullGameList(List<GameRoomData> gameList) {
			using (var bytes = new MemoryStream()) {
				ListProxy<GameRoomData>.Serialize(bytes, gameList, GameRoomDataProxy.Serialize);

				SendEvent((byte)IGamePeerEventsType.FullGameList, bytes);
			}
		}

		public void SendGameListUpdate(List<GameRoomData> updatedGames, List<int> removedGames) {
			using (var bytes = new MemoryStream()) {
				ListProxy<GameRoomData>.Serialize(bytes, updatedGames, GameRoomDataProxy.Serialize);
				ListProxy<int>.Serialize(bytes, removedGames, Int32Proxy.Serialize);

				SendEvent((byte)IGamePeerEventsType.GameListUpdate, bytes);
			}
		}

		public void SendGameListUpdateEnd() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGamePeerEventsType.GameListUpdateEnd, bytes);
			}
		}

		public void SendGetGameInformation(GameRoomData room, List<GameActorInfo> players, int endTime) {
			using (var bytes = new MemoryStream()) {
				GameRoomDataProxy.Serialize(bytes, room);
				ListProxy<GameActorInfo>.Serialize(bytes, players, GameActorInfoProxy.Serialize);
				Int32Proxy.Serialize(bytes, endTime);

				SendEvent((byte)IGamePeerEventsType.GetGameInformation, bytes);
			}
		}

		public void SendServerLoadData(PhotonServerLoad data) {
			using (var bytes = new MemoryStream()) {
				PhotonServerLoadProxy.Serialize(bytes, data);

				SendEvent((byte)IGamePeerEventsType.ServerLoadData, bytes);
			}
		}

		public void SendDisconnectAndDisablePhoton(string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)IGamePeerEventsType.DisconnectAndDisablePhoton, bytes);
			}
		}

	}
}
