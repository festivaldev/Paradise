namespace Paradise.DataCenter.Common.Entities {
	public class GroupOperationResult {
		public const int Ok = 0;

		public const int InvalidName = 1;

		public const int AlreadyMemberOfAGroup = 2;

		public const int DuplicateName = 3;

		public const int InvalidTag = 4;

		public const int MemberNotFound = 5;

		public const int GroupNotFound = 6;

		public const int GroupFull = 7;

		public const int InvalidMotto = 8;

		public const int InvalidDescription = 9;

		public const int DuplicateTag = 10;

		public const int OffensiveName = 13;

		public const int OffensiveTag = 14;

		public const int OffensiveMotto = 15;

		public const int OffensiveDescription = 16;

		public const int IsNotOwner = 17;

		public const int NotEnoughRight = 18;

		public const int IsOwner = 19;

		public const int RequestNotFound = 20;

		public const int ExistingMemberRequest = 21;

		public const int InvitationNotFound = 23;

		public const int AlreadyInvited = 24;
	}
}
