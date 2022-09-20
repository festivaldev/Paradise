using System;
using System.Collections.Generic;

namespace Paradise.DataCenter.Common.Entities {
	public class LastIpView {
		public LastIpView(long ip, DateTime lastConnectionDate, List<LinkedMemberView> linkedMembers, BannedIpView bannedIpView) {
			this.Ip = ip;
			this.LastConnectionDate = lastConnectionDate;
			this.LinkedMembers = linkedMembers;
			this.BannedIpView = bannedIpView;
		}

		public long Ip { get; private set; }

		public DateTime LastConnectionDate { get; private set; }

		public List<LinkedMemberView> LinkedMembers { get; private set; }

		public BannedIpView BannedIpView { get; private set; }
	}
}
