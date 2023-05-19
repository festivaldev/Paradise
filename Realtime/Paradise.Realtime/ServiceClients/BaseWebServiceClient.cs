using log4net;
using Paradise.Realtime.Server;
using Paradise.Util.Ciphers;
using System;
using System.IO;
using System.Net;
using System.ServiceModel;

namespace Paradise.Realtime {
	public abstract class BaseWebServiceClient<T> {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(BaseWebServiceClient<T>));

		private readonly T _service;
		protected readonly ICryptographyPolicy NullCryptoPolicy = new NullCryptographyPolicy();
		protected readonly ICryptographyPolicy CryptoPolicy = new CryptographyPolicy();

		protected BaseWebServiceClient(string masterUrl, int port, string serviceEndpoint, string service) {
			if (string.IsNullOrWhiteSpace(masterUrl))
				throw new ArgumentNullException(nameof(masterUrl));

			if (string.IsNullOrWhiteSpace(serviceEndpoint))
				throw new ArgumentNullException(nameof(serviceEndpoint));

			if (string.IsNullOrWhiteSpace(service))
				throw new ArgumentNullException(nameof(service));

			var builder = new UriBuilder(masterUrl);
			builder.Path = Path.Combine(builder.Path, serviceEndpoint, service);
			builder.Port = port;

			BasicHttpBinding binding = new BasicHttpBinding {
				MaxBufferSize = ushort.MaxValue * 4,
				MaxReceivedMessageSize = ushort.MaxValue * 4
			};

			if (builder.Scheme.StartsWith("https")) {
				binding.Security.Mode = BasicHttpSecurityMode.Transport;
				binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
				ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
			}

			ChannelFactory<T> factory = new ChannelFactory<T>(binding);

			var address = new EndpointAddress(builder.Uri);
			_service = factory.CreateChannel(address);
		}

		protected T Service => _service;

		protected byte[] Encrypt(byte[] data) {
			if (string.IsNullOrWhiteSpace(BaseRealtimeApplication.Instance.EncryptionPassPhrase) || string.IsNullOrWhiteSpace(BaseRealtimeApplication.Instance.EncryptionInitVector)) {
				return NullCryptoPolicy.RijndaelEncrypt(data, "", "");
			}

			return CryptoPolicy.RijndaelEncrypt(data, BaseRealtimeApplication.Instance.EncryptionPassPhrase, BaseRealtimeApplication.Instance.EncryptionInitVector);
		}

		protected byte[] Decrypt(byte[] data) {
			if (string.IsNullOrWhiteSpace(BaseRealtimeApplication.Instance.EncryptionPassPhrase) || string.IsNullOrWhiteSpace(BaseRealtimeApplication.Instance.EncryptionInitVector)) {
				return NullCryptoPolicy.RijndaelDecrypt(data, "", "");
			}

			return CryptoPolicy.RijndaelDecrypt(data, BaseRealtimeApplication.Instance.EncryptionPassPhrase, BaseRealtimeApplication.Instance.EncryptionInitVector);
		}
	}
}
