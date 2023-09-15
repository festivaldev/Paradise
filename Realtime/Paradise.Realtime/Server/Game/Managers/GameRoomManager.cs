using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class GameRoomManager : IDisposable {
		private static readonly ILog Log = LogManager.GetLogger(nameof(GameRoomManager));

		private static readonly ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		private bool IsDisposed;

		private readonly BalancingLoopScheduler LoopScheduler = new BalancingLoopScheduler(64);

		public Dictionary<int, BaseGameRoom> Rooms { get; private set; } = new Dictionary<int, BaseGameRoom>();
		private readonly List<GameRoomData> UpdatedRooms = new List<GameRoomData>();
		private readonly List<int> RemovedRooms = new List<int>();

		private readonly object Lock = new object();

		public BaseGameRoom CreateRoom(GameRoomData data, string password) {
			if (data == null) {
				throw new ArgumentNullException(nameof(data));
			}

			if (ProfanityFilter.ContainsProfanity(data.Name)) {
				data.Name = ProfanityFilter.CensorString(data.Name);
			}

			/* Set those to 0, so the client knows there is no level restriction. */
			if (data.LevelMin <= 1) {
				data.LevelMin = 0;
			}
			if (data.LevelMax >= 100) {
				data.LevelMax = 0;
			}

			BaseGameRoom room = null;
			try {
				switch (data.GameMode) {
					case GameModeType.DeathMatch:
						room = new DeathMatchRoom(data, LoopScheduler);
						break;
					case GameModeType.TeamDeathMatch:
						room = new TeamDeathMatchRoom(data, LoopScheduler);
						break;
					case GameModeType.EliminationMode:
						room = new TeamEliminationRoom(data, LoopScheduler);
						break;
					default:
						throw new NotSupportedException();
				}
			} catch {
				room?.Dispose();
				throw;
			}

			lock (Lock) {
				var random = new Random((int)DateTime.UtcNow.Ticks);

				var roomId = random.Next(1, int.MaxValue);
				while (Rooms.ContainsKey(roomId)) {
					roomId = random.Next(1, int.MaxValue);
				}

				room.RoomId = roomId;
				room.Password = password;

				Rooms.Add(room.RoomId, room);
				UpdatedRooms.Add(room.MetaData);

				Log.Info($"Created {room}({room.RoomId})");

				if (GameServerApplication.Instance.Configuration.DiscordGameAnnouncements) {
					GameServerApplication.Instance.Socket?.SendSync(TcpSocket.PacketType.RoomOpened, room.MetaData);
				}
			}

			return room;
		}

		public bool TryGetRoom(int roomId, out BaseGameRoom room) {
			return Rooms.TryGetValue(roomId, out room);
		}

		public void RemoveRoom(int roomId) {
			if (Rooms.TryGetValue(roomId, out var room)) {
				Log.Info($"Destroyed {Rooms[roomId]}({Rooms[roomId].RoomId})");

				if (GameServerApplication.Instance.Configuration.DiscordGameAnnouncements) {
					GameServerApplication.Instance.Socket?.SendSync(TcpSocket.PacketType.RoomClosed, room.MetaData);
				}

				Rooms[roomId].Dispose();
				Rooms.Remove(roomId);
			}
		}


		public void Dispose() {
			if (IsDisposed) return;

			LoopScheduler.Dispose();
			IsDisposed = true;
		}

		public GamePeer FindPeerWithCmid(int cmid) {
			foreach (var room in Rooms) {
				foreach (var peer in room.Value.Peers) {
					if (peer.Actor.Cmid == cmid) {
						return peer;
					}
				}
			}

			return null;
		}
	}
}