using System.ServiceModel;

namespace Paradise.WebServices.LegacyWebServices {
	[ServiceContract]
	public interface IModerationWebServiceContract {
		[OperationContract]
		byte[] BanPermanently(byte[] data);
	}
}
