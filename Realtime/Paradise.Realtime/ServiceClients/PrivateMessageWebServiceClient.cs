using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime {
	public class PrivateMessageWebServiceClient : BaseWebServiceClient<IPrivateMessageWebServiceContract> {
		public static readonly PrivateMessageWebServiceClient Instance;

		static PrivateMessageWebServiceClient() {
			Instance = new PrivateMessageWebServiceClient(
				masterUrl: BaseRealtimeApplication.Instance.Configuration.MasterServerUrl,
				port: BaseRealtimeApplication.Instance.Configuration.WebServicePort,
				serviceEndpoint: BaseRealtimeApplication.Instance.Configuration.WebServiceEndpoint,
				webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
				webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
			);
		}

		public PrivateMessageWebServiceClient(string masterUrl, int port, string serviceEndpoint, string webServicePrefix, string webServiceSuffix) : base(masterUrl, port, serviceEndpoint, $"{webServicePrefix}PrivateMessageWebService{webServiceSuffix}") { }

		public int DeleteThread(string authToken, int otherCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, otherCmid);

				var result = Service.DeleteThread(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public List<MessageThreadView> GetAllMessageThreadsForUser(string authToken, int pageNumber) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageNumber);

				var result = Service.GetAllMessageThreadsForUser(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<MessageThreadView>.Deserialize(inputStream, MessageThreadViewProxy.Deserialize);
				}
			}
		}

		public PrivateMessageView GetMessageWithIdForCmid(string authToken, int messageId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, messageId);

				var result = Service.GetMessageWithIdForCmid(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return PrivateMessageViewProxy.Deserialize(inputStream);
				}
			}
		}

		public List<PrivateMessageView> GetThreadMessages(string authToken, int otherCmid, int pageNumber) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, otherCmid);
				Int32Proxy.Serialize(bytes, pageNumber);

				var result = Service.GetThreadMessages(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<PrivateMessageView>.Deserialize(inputStream, PrivateMessageViewProxy.Deserialize);
				}
			}
		}

		public int MarkThreadAsRead(string authToken, int otherCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, otherCmid);

				var result = Service.MarkThreadAsRead(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public PrivateMessageView SendMessage(string authToken, int receiverCmid, string content) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, receiverCmid);
				StringProxy.Serialize(bytes, content);

				var result = Service.MarkThreadAsRead(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return PrivateMessageViewProxy.Deserialize(inputStream);
				}
			}
		}
	}
}
