using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract(Name = "IClanWebServiceContract")]
	public interface IClanWebServiceContract_Legacy : IWebServiceContractBase {
		[OperationContract]
		byte[] AcceptClanInvitation(byte[] data);

		[OperationContract]
		byte[] CancelInvitation(byte[] data);

		[OperationContract]
		byte[] CancelRequest(byte[] data);

		[OperationContract]
		byte[] CanOwnAClan(byte[] data);

		[OperationContract]
		byte[] CreateClan(byte[] data);

		[OperationContract]
		byte[] DeclineClanInvitation(byte[] data);

		[OperationContract]
		byte[] DisbandGroup(byte[] data);

		[OperationContract]
		byte[] GetAllGroupInvitations(byte[] data);

		[OperationContract]
		byte[] GetClan(byte[] data);

		[OperationContract]
		byte[] GetMyClanId(byte[] data);

		[OperationContract]
		byte[] GetPendingGroupInvitations(byte[] data);

		[OperationContract]
		byte[] InviteMemberToJoinAGroup(byte[] data);

		[OperationContract]
		byte[] IsMemberPartOfAnyGroup(byte[] data);

		[OperationContract]
		byte[] IsMemberPartOfGroup(byte[] data);

		[OperationContract]
		byte[] KickMemberFromClan(byte[] data);

		[OperationContract]
		byte[] LeaveAClan(byte[] data);

		[OperationContract]
		byte[] test(byte[] data);

		[OperationContract]
		byte[] TransferOwnership(byte[] data);

		[OperationContract]
		byte[] UpdateMemberPosition(byte[] data);
	}
}