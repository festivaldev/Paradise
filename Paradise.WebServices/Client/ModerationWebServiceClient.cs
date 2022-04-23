using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System.IO;

namespace Paradise.WebServices.Client {
	public class ModerationWebServiceClient : WebServiceClientBase<IModerationWebServiceContract> {
		public ModerationWebServiceClient(string endpointUrl) : base(endpointUrl, $"{Properties.Resources.WebServicePrefix}ModerationWebService{Properties.Resources.WebServiceSuffix}") { }

		public MemberOperationResult OpPlayer(string authToken, int targetCmid, MemberAccessLevel accessLevel) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				EnumProxy<MemberAccessLevel>.Serialize(bytes, accessLevel);

				var result = Service.OpPlayer(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult DeopPlayer(string authToken, int targetCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);

				var result = Service.OpPlayer(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult BanPermanently(string authToken, int targetCmid, string reason) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				StringProxy.Serialize(bytes, reason);

				var result = Service.BanPermanently(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult UnbanPlayer(string authToken, int targetCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);

				var result = Service.UnbanPlayer(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}
	}
}
