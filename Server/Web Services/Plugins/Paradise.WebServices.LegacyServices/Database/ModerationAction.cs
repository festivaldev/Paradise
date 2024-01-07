using System;

namespace Paradise.WebServices.LegacyServices {
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
