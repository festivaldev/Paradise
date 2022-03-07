using System;

namespace Paradise.Core.Models {
	[Serializable]
	public class PlayerMovement {
		public byte Number { get; set; }

		public ShortVector3 Position { get; set; }

		public ShortVector3 Velocity { get; set; }

		public byte HorizontalRotation { get; set; }

		public byte VerticalRotation { get; set; }

		public byte KeyState { get; set; }

		public byte MovementState { get; set; }
	}
}
