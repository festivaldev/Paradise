using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Realtime.Server.Game {
	public partial class GameActor {
		public GameActorInfo Info { get; private set; }
		public PlayerMovement Movement { get; private set; }
		public DamageEvent Damage { get; private set; }

		public GameActorInfoDelta Delta => Info.Delta;

		public TeamID Team {
			get { return Info.TeamID; }
			set { Info.TeamID = value; }
		}

		public int Number {
			get { return Info.PlayerId; }
			set {
				Info.PlayerId = (byte)value;
				Movement.Number = (byte)value;
			}
		}

		public int Cmid => Info.Cmid;
		public string PlayerName => Info.PlayerName;
		public MemberAccessLevel AccessLevel => Info.AccessLevel;

		public GameActor(GameActorInfo actorInfo) {
			if (actorInfo == null) {
				throw new ArgumentNullException(nameof(actorInfo));
			}

			Info = actorInfo;
			Movement = new PlayerMovement();
			Damage = new DamageEvent();

			Info.Delta.Id = actorInfo.PlayerId;
		}
	}
}
