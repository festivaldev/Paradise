﻿using System;
using UberStrike.Core.Models;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public partial class BaseGameRoom {
		public class PlayerRespawnedEventArgs : EventArgs {
			public GamePeer Player { get; set; }
		}

		public class PlayerJoinedEventArgs : EventArgs {
			public GamePeer Player { get; set; }
			public TeamID Team { get; set; }
		}

		public class PlayerLeftEventArgs : EventArgs {
			public GamePeer Player { get; set; }
		}

		public class PlayerKilledEventArgs : EventArgs {
			public int VictimCmid { get; set; }
			public int AttackerCmid { get; set; }
			public UberstrikeItemClass ItemClass { get; set; }
			public ushort Damage { get; set; }
			public BodyPart Part { get; set; }
			public Vector3 Direction { get; set; }
		}
	}
}
