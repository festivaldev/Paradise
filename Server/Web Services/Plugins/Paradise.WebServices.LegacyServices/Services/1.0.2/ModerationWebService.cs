using log4net;
using System;
using System.IO;
using System.ServiceModel;
using UberStrike.Core.Serialization.Legacy;

namespace Paradise.WebServices.LegacyServices._102 {
	public class ModerationWebService : BaseWebService, IModerationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(ModerationWebService));

		public override string ServiceName => "ModerationWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IModerationWebServiceContract);

		public ModerationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		#region IModerationWebServiceContract
		public byte[] BanPermanently(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var sourceCmid = Int32Proxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var applicationId = Int32Proxy.Deserialize(bytes);
					var ip = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), sourceCmid, targetCmid, applicationId, ip);

					using (var outputStream = new MemoryStream()) {
						throw new NotImplementedException();

						//return isEncrypted 
						//	? CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector) 
						//	: outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}
		#endregion
	}
}
