using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract]
	public interface IModerationWebServiceContract {
		[OperationContract]
		byte[] OpPlayer(byte[] data);

		[OperationContract]
		byte[] DeopPlayer(byte[] data);

		[OperationContract]
		byte[] BanPermanently(byte[] data);

		[OperationContract]
		byte[] UnbanPlayer(byte[] data);
	}
}
