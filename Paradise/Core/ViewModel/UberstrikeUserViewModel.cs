using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class UberstrikeUserViewModel {
		public MemberView CmuneMemberView { get; set; }

		public UberstrikeMemberView UberstrikeMemberView { get; set; }
	}
}
