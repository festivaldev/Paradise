using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract(Name = "IUserWebServiceContract")]
	public interface IUserWebServiceContract_Legacy {
		[OperationContract]
		byte[] ChangeMemberName(byte[] data);

		[OperationContract]
		byte[] FindMembers(byte[] data);

		[OperationContract]
		byte[] GenerateNonDuplicatedMemberNames(byte[] data);

		[OperationContract]
		byte[] GetCurrencyDeposits(byte[] data);

		[OperationContract]
		byte[] GetInventory(byte[] data);

		[OperationContract]
		byte[] GetItemTransactions(byte[] data);

		[OperationContract]
		byte[] GetLevelCapsView(byte[] data);

		[OperationContract]
		byte[] GetLoadout(byte[] data);

		[OperationContract]
		byte[] GetMember(byte[] data);

		[OperationContract]
		byte[] GetMemberWallet(byte[] data);

		[OperationContract]
		byte[] GetPointsDeposits(byte[] data);

		[OperationContract]
		byte[] GetPublicProfile(byte[] data);

		[OperationContract]
		byte[] GetRealTimeStatistics(byte[] data);

		[OperationContract]
		byte[] GetStatistics(byte[] data);

		[OperationContract]
		byte[] GetUserAndTopStats(byte[] data);

		[OperationContract]
		byte[] GetXPEventsView(byte[] data);

		[OperationContract]
		byte[] IsDuplicateMemberName(byte[] data);

		[OperationContract]
		byte[] ReportMember(byte[] data);

		[OperationContract]
		byte[] SetLoadout(byte[] data);

		[OperationContract]
		byte[] SetScore(byte[] data);
	}
}
