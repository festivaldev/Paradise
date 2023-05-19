using System.ServiceModel;

namespace Paradise.WebServices.LegacyServices._102 {
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
		byte[] SetScore(byte[] data);

		[OperationContract]
		byte[] GetMember(byte[] data);

		[OperationContract]
		byte[] GetLoadout(byte[] data);

		[OperationContract]
		byte[] SetLoadout(byte[] data);

		[OperationContract]
		byte[] GetXPEventsView(byte[] data);

		[OperationContract]
		byte[] GetLevelCapsView(byte[] data);
	}
}
