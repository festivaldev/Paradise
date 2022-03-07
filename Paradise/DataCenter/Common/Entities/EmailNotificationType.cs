namespace Paradise.DataCenter.Common.Entities {
	public enum EmailNotificationType {
		DeleteMember = 1,
		BanMemberPermanent,
		MergeMembers,
		ChangeMemberName = 8,
		ChangeMemberPassword,
		ChangeMemberEmail = 11,
		BanMemberTemporary,
		UnbanMember,
		BanMemberChatPermanent,
		BanMemberChatTemporary,
		UnbanMemberChat,
		ChangeClanTag,
		ChangeClanName,
		ChangeClanMotto
	}
}
