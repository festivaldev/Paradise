using log4net;
using System;
using System.IO;
using System.Net;
using System.ServiceModel;

namespace Paradise.Realtime {
	public abstract class BaseWebServiceClient<T> {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(BaseWebServiceClient<T>));

		private readonly T _service;

		protected BaseWebServiceClient(string endpointUrl, string service) {
			if (endpointUrl == null)
				throw new ArgumentNullException(nameof(endpointUrl));

			var builder = new UriBuilder(endpointUrl);
			builder.Path = Path.Combine(builder.Path, service);

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
	}
}
