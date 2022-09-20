using System;

namespace Paradise.Core.Models {
	[Flags]
	public enum KeyState : byte {
		Still = 0,
		Forward = 1,
		Backward = 2,
		Left = 4,
		Right = 8,
		Jump = 16,
		Crouch = 32,
		Vertical = 3,
		Horizontal = 12,
		Walking = 15
	}
}
