using Newtonsoft.Json;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Realtime.Server.Game {
	[JsonObject(MemberSerialization.OptIn)]
	public partial class GameActor {
		[JsonProperty]
		public GameActorInfo Info { get; private set; }
		public PlayerMovement Movement { get; private set; }
		public DamageEvent Damage { get; private set; }

		public GamePeer Peer { get; set; }

		public GameActorInfoDelta Delta => Info.Delta;

		public TeamID Team {
			get { return Info.TeamID; }
			set { Info.TeamID = value; }
		}

		public DateTime LastRespawnTime = DateTime.UtcNow;
		public DateTime LastTeamSwitchTime = DateTime.UtcNow;

		[JsonProperty]
		public int Cmid => Info.Cmid;
		[JsonProperty]
		public string PlayerName => Info.PlayerName;
		[JsonProperty]
		public MemberAccessLevel AccessLevel => Info.AccessLevel;

		public bool UpdatePosition;

		public GameActor(GameActorInfo actorInfo) {
			if (actorInfo == null) {
				throw new ArgumentNullException(nameof(actorInfo));
			}

			Info = actorInfo;
			Movement = new PlayerMovement {
				Number = actorInfo.PlayerId
			};
			Damage = new DamageEvent();

			Info.Delta.Id = actorInfo.PlayerId;
		}
	}
}
