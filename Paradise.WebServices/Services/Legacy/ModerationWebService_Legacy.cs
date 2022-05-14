using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using Paradise.Core.Serialization.Legacy;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Paradise.Util.Ciphers;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.WebServices.Services {
	public class ModerationWebService_Legacy : WebServiceBase, IModerationWebServiceContract_Legacy {
		public override string ServiceName => "ModerationWebService";
		public override string ServiceVersion => "1.0.1";
		protected override Type ServiceInterface => typeof(IModerationWebServiceContract_Legacy);

		public ModerationWebService_Legacy(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public ModerationWebService_Legacy(BasicHttpBinding binding, ParadiseSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {}

		public byte[] BanPermanently(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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