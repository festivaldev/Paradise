using System.ServiceModel;

namespace Paradise.WebServices.LegacyServices._102 {
	[ServiceContract]
	public interface IPrivateMessageWebServiceContract {
		[OperationContract]
		byte[] GetAllMessageThreadsForUser_1(byte[] data);

		[OperationContract]
		byte[] GetAllMessageThreadsForUser_2(byte[] data);

		[OperationContract]
		byte[] GetThreadMessages(byte[] data);

		[OperationContract]
		byte[] SendMessage(byte[] data);

		[OperationContract]
		byte[] GetMessageWithId(byte[] data);

		[OperationContract]
		byte[] MarkThreadAsRead(byte[] data);

		[OperationContract]
		byte[] DeleteThread(byte[] data);
	}
}
