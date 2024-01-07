using System;

namespace Paradise.Realtime.Server.Game {
	[Flags]
	public enum GAME_FLAGS {
		None = 0,
		LowGravity = 1 << 0,
		NoArmor = 1 << 1,
		QuickSwitch = 1 << 2,
		MeleeOnly = 1 << 3
	}
}
