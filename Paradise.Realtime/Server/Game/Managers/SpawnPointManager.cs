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
		private int Index;
		private int SpawnCount;

		private readonly System.Random Rand = new System.Random((int)DateTime.UtcNow.Ticks);
		private readonly Dictionary<TeamID, List<SpawnPoint>> SpawnPointsByTeam = new Dictionary<TeamID, List<SpawnPoint>>();
		private readonly List<SpawnPoint> SpawnPoints = new List<SpawnPoint>();
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
			SpawnPoints.AddRange(spawns);
			Index = Rand.Next(SpawnPointsByTeam.Count);
		}

		public SpawnPoint Get(TeamID team) {
			if (team == TeamID.NONE) {
				if (SpawnCount % 5 == 0) {
					Index = Rand.Next(SpawnPoints.Count);
				} else {
					Index++;
				}

				return SpawnPoints[Index];
			} else {
				if (SpawnCount % 5 == 0) {
					Index = Rand.Next(SpawnPointsByTeam.Count);
				} else {
					Index++;
				}

				return SpawnPointsByTeam[team][Index];
			}
		}

		public int GetSpawnPointCount(TeamID team) {
			return SpawnPointsByTeam[team].Count;
		}

		public void Reset() {
			SpawnPointsInUse.Clear();
		}
	}
}
