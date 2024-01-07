using Cmune.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;
using UberStrike.Core.Serialization;

namespace Paradise.Realtime {
	public class RelationshipWebServiceClient : BaseWebServiceClient<IRelationshipWebServiceContract> {
		public static readonly RelationshipWebServiceClient Instance;

		static RelationshipWebServiceClient() {
			Instance = new RelationshipWebServiceClient(
				masterUrl: BaseRealtimeApplication.Instance.Configuration.MasterServerUrl,
				port: BaseRealtimeApplication.Instance.Configuration.WebServicePort,
				serviceEndpoint: BaseRealtimeApplication.Instance.Configuration.WebServiceEndpoint,
				webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
				webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
			);
		}

		public RelationshipWebServiceClient(string masterUrl, int port, string serviceEndpoint, string webServicePrefix, string webServiceSuffix) : base(masterUrl, port, serviceEndpoint, $"{webServicePrefix}RelationshipWebService{webServiceSuffix}") { }

		public PublicProfileView AcceptContactRequest(string authToken, int contactRequestId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, contactRequestId);

				var result = Service.AcceptContactRequest(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return PublicProfileViewProxy.Deserialize(inputStream);
				}
			}
		}

		public bool DeclineContactRequest(string authToken, int contactRequestId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, contactRequestId);

				var result = Service.DeclineContactRequest(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult DeleteContact(string authToken, int contactId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, contactId);

				var result = Service.DeleteContact(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public List<ContactRequestView> GetContactRequests(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetContactRequests(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<ContactRequestView>.Deserialize(inputStream, ContactRequestViewProxy.Deserialize);
				}
			}
		}

		public List<ContactGroupView> GetContactsByGroups(string authToken, bool populateFacebookIds) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				BooleanProxy.Serialize(bytes, populateFacebookIds);

				var result = Service.GetContactsByGroups(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<ContactGroupView>.Deserialize(inputStream, ContactGroupViewProxy.Deserialize);
				}
			}
		}

		public int SendContactRequest(string authToken, int receiverCmid, string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, receiverCmid);
				StringProxy.Serialize(bytes, message);

				var result = Service.GetContactsByGroups(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}
	}
}
