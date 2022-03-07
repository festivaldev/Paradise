using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;

namespace Paradise.WebServices.Client {
	public class RelationshipWebServiceClient : WebServiceClientBase<IRelationshipWebServiceContract> {
		public RelationshipWebServiceClient(string endpointUrl) : base(endpointUrl, $"{Properties.Resources.WebServicePrefix}RelationshipWebService{Properties.Resources.WebServiceSuffix}") { }

		public PublicProfileView AcceptContactRequest(string authToken, int contactRequestId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, contactRequestId);

				var result = Service.AcceptContactRequest(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return PublicProfileViewProxy.Deserialize(inputStream);
				}
			}
		}

		public bool DeclineContactRequest(string authToken, int contactRequestId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, contactRequestId);

				var result = Service.DeclineContactRequest(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult DeleteContact(string authToken, int contactId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, contactId);

				var result = Service.DeleteContact(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public List<ContactRequestView> GetContactRequests(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetContactRequests(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<ContactRequestView>.Deserialize(inputStream, ContactRequestViewProxy.Deserialize);
				}
			}
		}

		public List<ContactGroupView> GetContactsByGroups(string authToken, bool populateFacebookIds) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				BooleanProxy.Serialize(bytes, populateFacebookIds);

				var result = Service.GetContactsByGroups(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<ContactGroupView>.Deserialize(inputStream, ContactGroupViewProxy.Deserialize);
				}
			}
		}

		public int SendContactRequest(string authToken, int receiverCmid, string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, receiverCmid);
				StringProxy.Serialize(bytes, message);

				var result = Service.GetContactsByGroups(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}
	}
}
