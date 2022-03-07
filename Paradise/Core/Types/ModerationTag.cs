using System;

namespace Paradise.Core.Types {
	[Flags]
	public enum ModerationTag {
		None = 0,
		Muted = 1,
		Ghosted = 2,
		Banned = 4,
		Speedhacking = 8,
		Spamming = 16,
		Language = 32,
		Name = 64
	}
}
