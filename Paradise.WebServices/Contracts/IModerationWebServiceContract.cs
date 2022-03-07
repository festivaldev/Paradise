using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract]
	public interface IModerationWebServiceContract {
		//[OperationContract]
		//byte[] Ban(byte[] data);

		//[OperationContract]
		//byte[] UnbanCmid(byte[] data);

		//[OperationContract]
		//byte[] BanCmid(byte[] data);

		//[OperationContract]
		//byte[] BanIp(byte[] data);

		//[OperationContract]
		//byte[] BanHwd(byte[] data);

		[OperationContract]
		byte[] BanPermanently(byte[] data);
	}
}
