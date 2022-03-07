using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract(Name = "IRelationshipWebServiceContract")]
	public interface IRelationshipWebServiceContract_Legacy {
		[OperationContract]
		byte[] AcceptContactRequest(byte[] data);

		[OperationContract]
		byte[] DeclineContactRequest(byte[] data);

		[OperationContract]
		byte[] DeleteContact(byte[] data);

		[OperationContract]
		byte[] GetContactRequests(byte[] data);

		[OperationContract]
		byte[] GetContactsByGroups(byte[] data);

		[OperationContract]
		byte[] MoveContactToGroup(byte[] data);

		[OperationContract]
		byte[] SendContactRequest(byte[] data);
	}
}
