using System;

namespace Paradise.Core.ViewModel {
	[Serializable]
	public class ClanInvitationAnswerViewModel {
		public int ReturnValue { get; set; }

		public int GroupInvitationId { get; set; }

		public bool IsInvitationAccepted { get; set; }
	}
}
