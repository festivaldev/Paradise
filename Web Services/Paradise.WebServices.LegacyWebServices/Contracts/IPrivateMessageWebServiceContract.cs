using System.ServiceModel;

namespace Paradise.WebServices.LegacyWebServices {
	[ServiceContract]
	public interface IPrivateMessageWebServiceContract {
		[OperationContract]
		byte[] GetAllMessageThreadsForUser(byte[] data);

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
