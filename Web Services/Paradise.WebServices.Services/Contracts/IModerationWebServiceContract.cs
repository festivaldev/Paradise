using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract]
	public interface IModerationWebServiceContract {
		[OperationContract]
		byte[] OpPlayer(byte[] data);

		[OperationContract]
		byte[] DeopPlayer(byte[] data);

		//[OperationContract]
		//byte[] BanPermanently(byte[] data);

		//[OperationContract]
		//byte[] UnbanPlayer(byte[] data);

		//[OperationContract]
		//byte[] MutePlayer(byte[] data);

		//[OperationContract]
		//byte[] UnmutePlayer(byte[] data);

		//[OperationContract]
		//byte[] GetNaughtyList(byte[] data);

		[OperationContract]
		byte[] SetModerationFlag(byte[] data);

		[OperationContract]
		byte[] UnsetModerationFlag(byte[] data);

		[OperationContract]
		byte[] ClearModerationFlags(byte[] data);

		[OperationContract]
		byte[] GetNaughtyList(byte[] data);
	}
}
