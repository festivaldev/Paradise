using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;

namespace Paradise.WebServices.Client {
	public class PrivateMessageWebServiceClient : WebServiceClientBase<IPrivateMessageWebServiceContract> {
		public PrivateMessageWebServiceClient(string endpointUrl) : base(endpointUrl, $"{Properties.Resources.WebServicePrefix}PrivateMessageWebService{Properties.Resources.WebServiceSuffix}") { }

		public int DeleteThread(string authToken, int otherCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, otherCmid);

				var result = Service.DeleteThread(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public List<MessageThreadView> GetAllMessageThreadsForUser(string authToken, int pageNumber) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageNumber);

				var result = Service.GetAllMessageThreadsForUser(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<MessageThreadView>.Deserialize(inputStream, MessageThreadViewProxy.Deserialize);
				}
			}
		}

		public PrivateMessageView GetMessageWithIdForCmid(string authToken, int messageId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, messageId);

				var result = Service.GetMessageWithIdForCmid(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return PrivateMessageViewProxy.Deserialize(inputStream);
				}
			}
		}

		public List<PrivateMessageView> GetThreadMessages(string authToken, int otherCmid, int pageNumber) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, otherCmid);
				Int32Proxy.Serialize(bytes, pageNumber);

				var result = Service.GetThreadMessages(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<PrivateMessageView>.Deserialize(inputStream, PrivateMessageViewProxy.Deserialize);
				}
			}
		}

		public int MarkThreadAsRead(string authToken, int otherCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, otherCmid);

				var result = Service.MarkThreadAsRead(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public PrivateMessageView SendMessage(string authToken, int receiverCmid, string content) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, receiverCmid);
				StringProxy.Serialize(bytes, content);

				var result = Service.MarkThreadAsRead(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return PrivateMessageViewProxy.Deserialize(inputStream);
				}
			}
		}
	}
}
