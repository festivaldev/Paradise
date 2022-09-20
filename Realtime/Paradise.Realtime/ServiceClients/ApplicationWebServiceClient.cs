using Paradise.Core.Models.Views;
using Paradise.Core.Serialization;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime {
	public class ApplicationWebServiceClient : BaseWebServiceClient<IApplicationWebServiceContract> {
		public static readonly ApplicationWebServiceClient Instance = new ApplicationWebServiceClient(
			endpointUrl: BaseRealtimeApplication.Instance.Configuration.WebServiceBaseUrl,
			webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
			webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
		);

		public ApplicationWebServiceClient(string endpointUrl, string webServicePrefix, string webServiceSuffix) : base(endpointUrl, $"{webServicePrefix}ApplicationWebService{webServiceSuffix}") { }

		public AccountCompletionResultView AuthenticateApplication(string clientVersion, ChannelType channel, string publicKey) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, publicKey);

				var result = Service.AuthenticateApplication(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return AccountCompletionResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public ApplicationConfigurationView GetConfigurationData(string clientVersion) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);

				var result = Service.GetConfigurationData(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ApplicationConfigurationViewProxy.Deserialize(inputStream);
				}
			}
		}

		public List<MapView> GetMaps(string clientVersion, DefinitionType clientType) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);
				EnumProxy<DefinitionType>.Serialize(bytes, clientType);

				var result = Service.GetMaps(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<MapView>.Deserialize(inputStream, MapViewProxy.Deserialize);
				}
			}
		}

		public AccountCompletionResultView SetMatchScore(string clientVersion, MatchStats scoringView, string serverAuthentication) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);
				MatchStatsProxy.Serialize(bytes, scoringView);
				StringProxy.Serialize(bytes, serverAuthentication);

				var result = Service.SetMatchScore(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					throw new NotImplementedException();
				}
			}
		}
	}
}
