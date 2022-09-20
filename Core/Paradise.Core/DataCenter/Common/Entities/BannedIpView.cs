using System;

namespace Paradise.DataCenter.Common.Entities {
	public class BannedIpView {
		public BannedIpView(int bannedIpId, long ipAddress, DateTime? bannedUntil, DateTime banningDate, int sourceCmid, string sourceName, int targetCmid, string targetName, string reason) {
			this.BannedIpId = bannedIpId;
			this.IpAddress = ipAddress;
			this.BannedUntil = bannedUntil;
			this.BanningDate = banningDate;
			this.SourceCmid = sourceCmid;
			this.SourceName = sourceName;
			this.TargetCmid = targetCmid;
			this.TargetName = targetName;
			this.Reason = reason;
		}

		public int BannedIpId { get; private set; }

		public long IpAddress { get; private set; }

		public DateTime? BannedUntil { get; private set; }

		public DateTime BanningDate { get; private set; }

		public int SourceCmid { get; private set; }

		public string SourceName { get; private set; }

		public int TargetCmid { get; private set; }

		public string TargetName { get; private set; }

		public string Reason { get; set; }
	}
}
