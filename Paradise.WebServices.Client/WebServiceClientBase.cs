using System;
using System.IO;
using System.ServiceModel;

namespace Paradise.WebServices.Client {
	public abstract class WebServiceClientBase<T> {
		private readonly T _service;


		private static readonly BasicHttpBinding binding = new BasicHttpBinding {
			MaxBufferSize = ushort.MaxValue * 4,
			MaxReceivedMessageSize = ushort.MaxValue * 4
		};

		private static readonly ChannelFactory<T> factory = new ChannelFactory<T>(binding);

		protected WebServiceClientBase(string endPoint, string service) {
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			var builder = new UriBuilder(endPoint);
			builder.Path = Path.Combine(builder.Path, service);

			var address = new EndpointAddress(builder.Uri);
			_service = factory.CreateChannel(address);
		}

		protected T Service => _service;
	}
}
