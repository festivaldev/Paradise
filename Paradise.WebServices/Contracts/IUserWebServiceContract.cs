using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract]
	public interface IUserWebServiceContract {
		[OperationContract]
		byte[] ChangeMemberName(byte[] data);

		[OperationContract]
		byte[] IsDuplicateMemberName(byte[] data);

		[OperationContract]
		byte[] GenerateNonDuplicatedMemberNames(byte[] data);

		[OperationContract]
		byte[] GetMemberWallet(byte[] data);

		[OperationContract]
		byte[] GetInventory(byte[] data);

		[OperationContract]
		byte[] GetCurrencyDeposits(byte[] data);

		[OperationContract]
		byte[] GetItemTransactions(byte[] data);

		[OperationContract]
		byte[] GetPointsDeposits(byte[] data);

		[OperationContract]
		byte[] GetLoadout(byte[] data);

		[OperationContract]
		byte[] SetLoadout(byte[] data);

		[OperationContract]
		byte[] GetMember(byte[] data);

		[OperationContract]
		byte[] GetMemberSessionData(byte[] data);

		[OperationContract]
		byte[] GetMemberListSessionData(byte[] data);

		[OperationContract]
		byte[] AddItemTransaction(byte[] data);

		[OperationContract]
		byte[] DepositCredits(byte[] data);
		
		[OperationContract]
		byte[] DepositPoints(byte[] data);

		[OperationContract]
		byte[] UpdatePlayerStatistics(byte[] data);
	}
}