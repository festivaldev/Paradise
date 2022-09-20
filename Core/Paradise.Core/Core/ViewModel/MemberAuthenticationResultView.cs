using Paradise.DataCenter.Common.Entities;
using System;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class MemberAuthenticationResultView {
		public MemberAuthenticationResult MemberAuthenticationResult { get; set; }

		public MemberView MemberView { get; set; }

		public PlayerStatisticsView PlayerStatisticsView { get; set; }

		public DateTime ServerTime { get; set; }

		public bool IsAccountComplete { get; set; }

		public LuckyDrawUnityView LuckyDraw { get; set; }

		public string AuthToken { get; set; }

		public bool IsTutorialComplete { get; set; } // # LEGACY # //

		public WeeklySpecialView WeeklySpecial { get; set; }  // # LEGACY # //
	}
}
