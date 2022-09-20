using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.WebServices {
	public enum ModerationFlag {
		Muted = 0x1,
		Ghosted = 0x2,
		Banned = 0x4,
		Speed = 0x8,
		Spamming = 0x10,
		CrudeLanguage = 0x20
	}

	public class ModerationAction {
		//public ModerationActionType ActionType { get; set; }
		public ModerationFlag ModerationFlag { get; set; }
		public int SourceCmid { get; set; }
		public string SourceName { get; set; }
		public int TargetCmid { get; set; }
		public string TargetName { get; set; }
		public DateTime ActionDate { get; set; }
		public DateTime ExpireTime { get; set; }
		public string Reason { get; set; }

		public override string ToString() {
			return $"{TargetName}({TargetCmid}) [{ModerationFlag}({(int)ModerationFlag})] <- {SourceName}({SourceCmid}) (created: {ActionDate}, expires: {ExpireTime}) (reason: {Reason})";
		}
	}
}
