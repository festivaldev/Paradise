using log4net;
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
using System.Net;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class ApplicationWebService : BaseWebService, IApplicationWebServiceContract {
		public override string ServiceName => "ApplicationWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IApplicationWebServiceContract);

		private static readonly string[] supportedClientVersions = { "4.7.1" };

		private ApplicationConfigurationView applicationConfiguration;
		private AuthenticateApplicationView photonServers;
		private List<MapView> mapData;
		private List<UberstrikeCustomMapView> customMapData;

		public ApplicationWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public ApplicationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			try {
				applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "ApplicationConfiguration.json")));
				photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "PhotonServers.json")));
				mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "Maps.json")));
				customMapData = JsonConvert.DeserializeObject<List<UberstrikeCustomMapView>>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "CustomMaps.json")));

				// Resolve them domains
				try {
					photonServers.CommServer.IP = Dns.GetHostAddresses(photonServers.CommServer.IP).FirstOrDefault().ToString();
				} catch (Exception e) {
					Log.Error($"Failed to resolve CommServer IP: {e.Message}");
					Log.Debug(e);

					ServiceError?.Invoke(this, new ServiceEventArgs {
						ServiceName = ServiceName,
						ServiceVersion = ServiceVersion,
						Exception = e
					});
				}

				try {
					foreach (var gameServer in photonServers.GameServers) {
						gameServer.IP = Dns.GetHostAddresses(gameServer.IP).FirstOrDefault().ToString();
					}
				} catch (Exception e) {
					Log.Error($"Failed to resolve GameServer IP: {e.Message}");
					Log.Debug(e);

					ServiceError?.Invoke(this, new ServiceEventArgs {
						ServiceName = ServiceName,
						ServiceVersion = ServiceVersion,
						Exception = e
					});
				}
			} catch (Exception e) {
				Log.Error($"Failed to load {ServiceName} data: {e.Message}");
				Log.Debug(e);

				ServiceError?.Invoke(this, new ServiceEventArgs {
					ServiceName = ServiceName,
					ServiceVersion = ServiceVersion,
					Exception = e
				});
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
						if (channel != ChannelType.Steam && 
							channel != ChannelType.WindowsStandalone && 
							channel != ChannelType.OSXStandalone) {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = false
							});
						} else {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = true,
								GameServers = photonServers.GameServers,
								CommServer = photonServers.CommServer,
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
						ApplicationConfigurationViewProxy.Serialize(outputStream, applicationConfiguration);

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
		/// Gets a list of custom maps that have been made available for the game
		/// </summary>
		/// <see cref="MapView"/>
		public byte[] GetCustomMaps(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var clientType = EnumProxy<DefinitionType>.Deserialize(bytes);

					DebugEndpoint(clientVersion, clientType);

					if (!supportedClientVersions.Contains(clientVersion))
						return null;

					using (var outputStream = new MemoryStream()) {
						ListProxy<UberstrikeCustomMapView>.Serialize(outputStream, customMapData, UberstrikeCustomMapViewProxy.Serialize);

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

						//return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}
	}
}
