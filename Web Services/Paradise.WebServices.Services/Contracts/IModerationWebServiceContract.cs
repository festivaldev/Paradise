using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract]
	public interface IModerationWebServiceContract {
		[OperationContract]
		byte[] BanPermanently(byte[] data);

		// CUSTOM
		[OperationContract]
		byte[] SetModerationFlag(byte[] data);

		// CUSTOM
		[OperationContract]
		byte[] UnsetModerationFlag(byte[] data);

		// CUSTOM
		[OperationContract]
		byte[] ClearModerationFlags(byte[] data);

		// CUSTOM
		[OperationContract]
		byte[] GetNaughtyList(byte[] data);
	}
}
