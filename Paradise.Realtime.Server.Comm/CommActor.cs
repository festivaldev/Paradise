﻿using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Realtime.Server.Comm {
	public class CommActor {
		public CommPeer Peer { get; }
		public CommActorInfo View { get; }

		public bool IsMuted { get; set; }
		public DateTime MuteEndTime { get; set; }

		public int Cmid => View.Cmid;
		public string Name => View.PlayerName;
		public MemberAccessLevel AccessLevel => View.AccessLevel;

		public CommActor(CommPeer peer, CommActorInfo view) {
			Peer = peer ?? throw new ArgumentNullException(nameof(peer));
			View = view ?? throw new ArgumentNullException(nameof(view));
		}
	}
}
