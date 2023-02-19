using System.ServiceModel;

namespace Paradise.WebServices.LegacyWebServices {
	[ServiceContract]
	public interface IAuthenticationWebServiceContract {
		[OperationContract]
		byte[] CreateUser(byte[] data);

		[OperationContract]
		byte[] CompleteAccount(byte[] data);

		[OperationContract]
		byte[] LoginMemberEmail(byte[] data);

		[OperationContract]
		byte[] LoginMemberCookie(byte[] data);

		[OperationContract]
		byte[] LoginMemberFacebook(byte[] data);

		[OperationContract]
		byte[] FacebookSingleSignOn(byte[] data);
	}
}
