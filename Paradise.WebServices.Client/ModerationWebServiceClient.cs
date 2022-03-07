using Paradise.Core.Serialization;
using Paradise.WebServices.Contracts;
using System.IO;

namespace Paradise.WebServices.Client {
	public class ModerationWebServiceClient : WebServiceClientBase<IModerationWebServiceContract> {
		public ModerationWebServiceClient(string endpointUrl) : base(endpointUrl, $"{Properties.Resources.WebServicePrefix}ModerationWebService{Properties.Resources.WebServiceSuffix}") { }

		public int BanPermanently(string authToken, int targetCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);

				var result = Service.BanPermanently(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}
	}
}
