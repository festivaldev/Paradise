using log4net;
using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public struct SpawnPoint {
		public Vector3 Position;
		public byte Rotation;

		public SpawnPoint(Vector3 position, byte rotation) {
			Position = position;
			Rotation = rotation;
		}

		public override string ToString() {
			return $"{Position}:{Rotation}";
		}
	}

	public class SpawnPointManager {
		private static readonly ILog Log = LogManager.GetLogger("GameLog");

		private int Index;

		private readonly System.Random Rand = new System.Random((int)DateTime.UtcNow.Ticks);
		private readonly Dictionary<TeamID, List<SpawnPoint>> SpawnPointsByTeam = new Dictionary<TeamID, List<SpawnPoint>>();
		public readonly Dictionary<TeamID, List<SpawnPoint>> SpawnPointsInUse = new Dictionary<TeamID, List<SpawnPoint>>();

		public bool IsLoaded(TeamID team) => SpawnPointsByTeam.ContainsKey(team);

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

		public SpawnPoint Get(TeamID team) {
			Index = Rand.Next(SpawnPointsByTeam[team].Count);
			return SpawnPointsByTeam[team][Index];
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
