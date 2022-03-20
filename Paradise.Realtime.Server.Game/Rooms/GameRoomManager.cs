using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Paradise.Realtime.Server.Game {
	public class GameRoomManager : IDisposable {
		private static readonly ILog Log = LogManager.GetLogger(typeof(GameRoomManager));

		private int LastRoomId;
		private bool IsDisposed;

		private readonly BalancingLoopScheduler LoopScheduler = new BalancingLoopScheduler(64);

		public Dictionary<int, BaseGameRoom> Rooms { get; private set; } = new Dictionary<int, BaseGameRoom>();
		private readonly List<GameRoomData> UpdatedRooms = new List<GameRoomData>();
		private readonly List<int> RemovedRooms = new List<int>();

		private object Lock = new object();

		public BaseGameRoom CreateRoom(GameRoomData data, string password) {
			if (data == null) {
				throw new ArgumentNullException(nameof(data));
			}

			/* Set those to 0, so the client knows there is no level restriction. */
			if (data.LevelMin <= 1) {
				data.LevelMin = 0;
			}
			if (data.LevelMax >= 80) {
				data.LevelMax = 0;
			}

			BaseGameRoom room = null;
			try {
				//switch (data.GameMode) {
				//	case GameModeType.DeathMatch:
				//		//room = new DeathmatchGameRoom(data, LoopScheduler);
				//		//break;
				//	case GameModeType.TeamDeathMatch:
				//		//room = new TeamDeathmatchGameRoom(data, LoopScheduler);
				//		//break;
				//	case GameModeType.EliminationMode:
				//		//room = new TeamEliminationGameRoom(data, LoopScheduler);
				//		//break;
				//	default:
				//		throw new NotSupportedException();
				//}

				room = new TestGameRoom(data, LoopScheduler);
			} catch {
				room?.Dispose();
				throw;
			}

			lock (Lock) {
				room.RoomId = LastRoomId++;
				room.Password = password;

				Rooms.Add(room.RoomId, room);
				UpdatedRooms.Add(room.MetaData);

				Log.Info($"Created {room}({room.RoomId})");
			}

			return room;
		}

		public BaseGameRoom GetRoom(int roomId) {
			var room = default(BaseGameRoom);
			Rooms.TryGetValue(roomId, out room);
			return room;
		}

		public void RemoveRoom(int roomId) {
			if (Rooms.ContainsKey(roomId)) {
				Rooms[roomId].Dispose();
				Rooms.Remove(roomId);
			}
		}


		public void Dispose() {
			if (IsDisposed) return;

			LoopScheduler.Dispose();
			IsDisposed = true;
		}
	}
}