using System;

namespace Paradise.Core.Models {
	[Flags]
	public enum MoveStates : byte {
		None = 0,
		Grounded = 1,
		Jumping = 2,
		Flying = 4,
		Ducked = 8,
		Wading = 16,
		Swimming = 32,
		Diving = 64,
		Landed = 128
	}
}
