using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using Paradise.Core.Serialization;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class ApplicationWebService : WebServiceBase, IApplicationWebServiceContract {
		protected override string ServiceName => "ApplicationWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IApplicationWebServiceContract);

		private static string[] supportedClientVersions = { "4.7.1", "4.8.8" };

		private ApplicationConfigurationView gameConfiguration;
		private List<MapView> mapData;
		private AuthenticateApplicationView defaultAppAuthentication;

		public ApplicationWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() {
			try {
				gameConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "ApplicationConfiguration.json")));
				defaultAppAuthentication = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "ApplicationData.json")));
				mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Maps.json")));
			} catch (Exception e) {
				Log.Error($"Failed to load {ServiceName} data: {e.Message}");
			}
		}

		/// <summary>
		/// Checks if an application is running the Steam build of v4.7.1
		/// </summary>
		public byte[] AuthenticateApplication(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var publicKey = StringProxy.Deserialize(bytes);

					DebugEndpoint(clientVersion, channel, publicKey);

					using (var outputStream = new MemoryStream()) {
						if (channel != ChannelType.Steam && channel != ChannelType.WindowsStandalone) {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = false
							});
						} else {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = true,
								GameServers = defaultAppAuthentication.GameServers,
								CommServer = defaultAppAuthentication.CommServer,
								WarnPlayer = !supportedClientVersions.Contains(clientVersion)
							});
						}

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Sends game configuration data to the client, such as XP required for a certain level or rewarded XP for different types of kills
		/// </summary>
		/// <see cref="ApplicationConfigurationView"/>
		public byte[] GetConfigurationData(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clientVersion = StringProxy.Deserialize(bytes);

					DebugEndpoint(clientVersion);

					if (!supportedClientVersions.Contains(clientVersion))
						return null;

					using (var outputStream = new MemoryStream()) {
						ApplicationConfigurationViewProxy.Serialize(outputStream, gameConfiguration);

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Gets a list of maps and their configuration per gamemode
		/// </summary>
		/// <see cref="MapView"/>
		public byte[] GetMaps(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var clientType = EnumProxy<DefinitionType>.Deserialize(bytes);

					DebugEndpoint(clientVersion, clientType);

					if (!supportedClientVersions.Contains(clientVersion))
						return null;

					using (var outputStream = new MemoryStream()) {
						ListProxy<MapView>.Serialize(outputStream, mapData, MapViewProxy.Serialize);

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Sets the score of a match, probably after a finished game round. Appears to be currently unused.
		/// </summary>
		public byte[] SetMatchScore(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var scoringView = MatchStatsProxy.Deserialize(bytes);
					var serverAuthentication = StringProxy.Deserialize(bytes);

					DebugEndpoint(clientVersion, scoringView, serverAuthentication);

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
