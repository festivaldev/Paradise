using System;
using System.IO;
using System.Net;
using System.ServiceModel;

namespace Paradise.WebServices.Client {
	public abstract class WebServiceClientBase<T> {
		private readonly T _service;

		protected WebServiceClientBase(string endPoint, string service) {
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			var builder = new UriBuilder(endPoint);
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
