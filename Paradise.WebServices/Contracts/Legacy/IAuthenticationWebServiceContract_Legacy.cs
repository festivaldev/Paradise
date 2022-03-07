using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract(Name = "IAuthenticationWebServiceContract")]
	public interface IAuthenticationWebServiceContract_Legacy : IWebServiceContractBase {
		[OperationContract]
		byte[] CompleteAccount(byte[] data);

		[OperationContract]
		byte[] CreateUser(byte[] data);

		[OperationContract]
		byte[] LoginMemberCookie(byte[] data);

		[OperationContract]
		byte[] LoginMemberEmail(byte[] data);

		[OperationContract]
		byte[] UncompleteAccount(byte[] data);
	}
}