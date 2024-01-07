using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using UberStrike.Core.Models;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public struct SpawnPoint {
		public Vector3 Position;
		public Vector2 Rotation;

		public SpawnPoint(Vector3 position, Vector2 rotation) {
			Position = position;
			Rotation = rotation;
		}

		public SpawnPoint(Vector3 position, byte rotation) {
			Position = position;
			Rotation = new Vector2(0, Conversion.Byte2Angle(rotation));
		}

		public override string ToString() {
			return $"SpawnPoint[position:{Position}, rotation:{Rotation}]";
		}
	}

	public class SpawnPointManager {
		private static readonly ILog Log = LogManager.GetLogger(nameof(SpawnPointManager));

		private readonly System.Random Rand = new System.Random((int)DateTime.UtcNow.Ticks);

		private readonly Dictionary<TeamID, List<SpawnPoint>> SpawnPointsByTeam = new Dictionary<TeamID, List<SpawnPoint>>();
		public bool IsLoaded(TeamID team) => SpawnPointsByTeam.ContainsKey(team);

		public Dictionary<TeamID, List<SpawnPoint>> SpawnPointsInUse => Room.Players
			.GroupBy(p => p.Actor.Team)
			.ToDictionary(g => g.Key, g => g.Select(p => p.Actor.CurrentSpawnPoint)
			.ToList());

		private readonly BaseGameRoom Room;

		public SpawnPointManager(BaseGameRoom room) {
			Room = room ?? throw new ArgumentNullException(nameof(room));
		}

		public void Load(TeamID team, List<Vector3> positions, List<byte> rotations) {
			if (positions == null) {
				throw new ArgumentNullException(nameof(positions));
			}

			if (rotations == null) {
				throw new ArgumentNullException(nameof(rotations));
			}

			int length = positions.Count;
			var spawns = new List<SpawnPoint>(length);
			for (int i = 0; i < length; i++) {
				spawns.Add(new SpawnPoint(positions[i], rotations[i]));
			}

			SpawnPointsByTeam[team] = spawns;
		}

		public bool TryGet(TeamID team, out SpawnPoint spawnPoint) {
			spawnPoint = new SpawnPoint { Position = Vector3.zero, Rotation = Vector2.zero };

			if (!SpawnPointsByTeam.ContainsKey(team)) return false;

			var index = Rand.Next(SpawnPointsByTeam[team].Count);
			//Log.Info($"spawn point for team:{team} index:{index}");

			if (SpawnPointsByTeam[team].Count() <= index) return false;

			spawnPoint = SpawnPointsByTeam[team][index];
			return true;
		}

		public int GetSpawnPointCount(TeamID team) {
			return SpawnPointsByTeam[team].Count;
		}

		public bool IsSpawnPointInUse(TeamID team, SpawnPoint spawnPoint) {
			if (SpawnPointsInUse[team] == null) return false;

			foreach (var point in SpawnPointsInUse[team]) {
				if (point.Position.Equals(spawnPoint.Position) && point.Rotation.Equals(spawnPoint.Rotation)) {
					return true;
				}
			}

			return false;
		}

		public void Reset() {
			foreach (var team in SpawnPointsInUse.Keys) {
				SpawnPointsInUse[team].Clear();
			}
		}
	}
}
