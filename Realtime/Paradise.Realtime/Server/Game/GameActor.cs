using Newtonsoft.Json;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	[JsonObject(MemberSerialization.OptIn)]
	public partial class GameActor {
		[JsonProperty]
		public GameActorInfo ActorInfo { get; private set; }
		public PlayerMovement Movement { get; private set; }
		public DamageEvent Damage { get; private set; }

		public GamePeer Peer { get; set; }

		public GameActorInfoDelta Delta => ActorInfo.Delta;

		public TeamID Team {
			get { return ActorInfo.TeamID; }
			set { ActorInfo.TeamID = value; }
		}

		public SpawnPoint CurrentSpawnPoint;
		public List<SpawnPoint> PreviousSpawnPoints = new List<SpawnPoint>();

		public DateTime LastRespawnTime = DateTime.UtcNow;
		public DateTime LastTeamSwitchTime = DateTime.UtcNow;
		public DateTime NextRespawnTime = DateTime.MinValue;

		[JsonProperty]
		public int Cmid => ActorInfo.Cmid;
		[JsonProperty]
		public string PlayerName => ActorInfo.PlayerName;
		[JsonProperty]
		public MemberAccessLevel AccessLevel => ActorInfo.AccessLevel;

		public bool UpdatePosition;

		public GameActor(GameActorInfo actorInfo) {
			ActorInfo = actorInfo ?? throw new ArgumentNullException(nameof(actorInfo));
			Movement = new PlayerMovement {
				Number = actorInfo.PlayerId
			};
			Damage = new DamageEvent();

			ActorInfo.Delta.Id = actorInfo.PlayerId;
		}
	}
}
