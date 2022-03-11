using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Realtime.Server.Comm {
	public class CommActor {
		public CommPeer Peer { get; private set; }
		public CommActorInfo ActorInfo { get; private set; }

		public int Cmid => ActorInfo.Cmid;
		public string Name => ActorInfo.PlayerName;
		public MemberAccessLevel AccessLevel => ActorInfo.AccessLevel;

		public DateTime MuteEndTime { get; set; }
		public bool IsMuted {
			get {
				return MuteEndTime >= DateTime.UtcNow;
			}
		}

		public CommActor(CommPeer peer, CommActorInfo actorInfo) {
			Peer = peer ?? throw new ArgumentNullException(nameof(peer));
			ActorInfo = actorInfo ?? throw new ArgumentNullException(nameof(actorInfo));
		}
	}
}
