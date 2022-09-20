using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class MemberAuthenticationViewModel {
		public MemberAuthenticationResult MemberAuthenticationResult { get; set; }

		public MemberView MemberView { get; set; }
	}
}
