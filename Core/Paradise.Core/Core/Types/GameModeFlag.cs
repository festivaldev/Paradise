using System;

namespace Paradise.Core.Types {
	[Flags]
	public enum GameModeFlag {
		None = 0,
		All = -1,
		DeathMatch = 1,
		TeamDeathMatch = 2,
		EliminationMode = 4
	}
}
