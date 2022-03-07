using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract(Name = "IModerationWebServiceContract")]
	public interface IModerationWebServiceContract_Legacy {
		[OperationContract]
		byte[] BanPermanently(byte[] data);
	}
}
