using Paradise.Core.Serialization;
using Paradise.WebServices.Contracts;
using System;
using System.IO;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class ModerationWebService : WebServiceBase, IModerationWebServiceContract {
		protected override string ServiceName => "ModerationWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IModerationWebServiceContract);

		public ModerationWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() { }

		//public byte[] Ban(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var serviceAuth = StringProxy.Deserialize(bytes);
		//			var cmid = Int32Proxy.Deserialize(bytes);

		//			using (var outputStream = new MemoryStream()) {
		//				throw new NotImplementedException();

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] BanCmid(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var cmid = Int32Proxy.Deserialize(bytes);

		//			using (var outputStream = new MemoryStream()) {
		//				throw new NotImplementedException();

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] BanHwd(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var hwd = StringProxy.Deserialize(bytes);

		//			using (var outputStream = new MemoryStream()) {
		//				throw new NotImplementedException();

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] BanIp(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var ip = Int32Proxy.Deserialize(bytes);

		//			using (var outputStream = new MemoryStream()) {
		//				throw new NotImplementedException();

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] UnbanCmid(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var cmid = Int32Proxy.Deserialize(bytes);

		//			using (var outputStream = new MemoryStream()) {
		//				throw new NotImplementedException();

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		/// <summary>
		/// Bans a user by Cmid permanently
		/// </summary>
		public byte[] BanPermanently(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, targetCmid);

					using (var outputStream = new MemoryStream()) {
						throw new NotImplementedException();

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}
	}
}
