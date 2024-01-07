using Cmune.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Serialization;
using UberStrike.Core.Types;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.Realtime {
	public class ApplicationWebServiceClient : BaseWebServiceClient<IApplicationWebServiceContract> {
		public static readonly ApplicationWebServiceClient Instance;

		static ApplicationWebServiceClient() {
			Instance = new ApplicationWebServiceClient(
				masterUrl: BaseRealtimeApplication.Instance.Configuration.MasterServerUrl,
				port: BaseRealtimeApplication.Instance.Configuration.WebServicePort,
				serviceEndpoint: BaseRealtimeApplication.Instance.Configuration.WebServiceEndpoint,
				webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
				webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
			);
		}

		public ApplicationWebServiceClient(string masterUrl, int port, string serviceEndpoint, string webServicePrefix, string webServiceSuffix) : base(masterUrl, port, serviceEndpoint, $"{webServicePrefix}ApplicationWebService{webServiceSuffix}") { }

		public AuthenticateApplicationView AuthenticateApplication(string clientVersion, ChannelType channel, string publicKey) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, publicKey);

				var result = Service.AuthenticateApplication(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return AuthenticateApplicationViewProxy.Deserialize(inputStream);
				}
			}
		}

		public ApplicationConfigurationView GetConfigurationData(string clientVersion) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);

				var result = Service.GetConfigurationData(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ApplicationConfigurationViewProxy.Deserialize(inputStream);
				}
			}
		}

		public List<MapView> GetMaps(string clientVersion, DefinitionType clientType) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);
				EnumProxy<DefinitionType>.Serialize(bytes, clientType);

				var result = Service.GetMaps(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<MapView>.Deserialize(inputStream, MapViewProxy.Deserialize);
				}
			}
		}

		public int SetMatchScore(string clientVersion, MatchStats scoringView, string serverAuthentication) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, clientVersion);
				MatchStatsProxy.Serialize(bytes, scoringView);
				StringProxy.Serialize(bytes, serverAuthentication);

				var result = Service.SetMatchScore(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					throw new NotImplementedException();
				}
			}
		}
	}
}
