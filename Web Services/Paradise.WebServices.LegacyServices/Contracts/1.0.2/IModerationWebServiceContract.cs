using System.ServiceModel;

namespace Paradise.WebServices.LegacyServices._102 {
	[ServiceContract]
	public interface IModerationWebServiceContract {
		[OperationContract]
		byte[] BanPermanently(byte[] data);
	}
}
