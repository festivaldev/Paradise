using Cmune.DataCenter.Common.Entities;
using log4net;
using Newtonsoft.Json;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Serialization;
using UberStrike.Core.Types;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.Services {
	public class ApplicationWebService : BaseWebService, IApplicationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(ApplicationWebService));

		public override string ServiceName => "ApplicationWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IApplicationWebServiceContract);

		private static readonly string[] supportedClientVersions = { "4.7.1" };
		private static readonly ChannelType[] supportedClientChannels = { ChannelType.Steam };

		private ApplicationConfigurationView applicationConfiguration;
		private AuthenticateApplicationView photonServers;
		private List<MapView> mapData;
		private List<ParadiseMapView> customMapData;

		private FileSystemWatcher watcher;
		private static readonly List<string> watchedFiles = new List<string> {
			"ApplicationConfiguration.json",
			"PhotonServers.json",
			"Maps.json",
			"CustomMaps.json"
		};

		public ApplicationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			try {
				applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "ApplicationConfiguration.json")));
				photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "PhotonServers.json")));
				mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Maps.json")));
				customMapData = JsonConvert.DeserializeObject<List<ParadiseMapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "CustomMaps.json")));

				watcher = new FileSystemWatcher(ServiceDataPath) {
					NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
				};

				watcher.Changed += (object sender, FileSystemEventArgs e) => {
					if (!watchedFiles.Contains(e.Name)) return;

					Task.Run(async () => {
						await Task.Delay(500);
						Log.Info("Reloading Application service data due to file changes.");

						applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "ApplicationConfiguration.json")));
						photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "PhotonServers.json")));
						mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Maps.json")));
						customMapData = JsonConvert.DeserializeObject<List<ParadiseMapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "CustomMaps.json")));
					});
				};

				watcher.EnableRaisingEvents = true;

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

		protected override void Teardown() {
			watcher.EnableRaisingEvents = false;
			watcher.Dispose();
		}

		#region IApplicationWebServiceContract
		/// <summary>
		/// Checks if an application is running the Steam build of v4.7.1
		/// </summary>
		public byte[] AuthenticateApplication(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var publicKey = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), clientVersion, channel, publicKey);

					using (var outputStream = new MemoryStream()) {
						if (!supportedClientChannels.Contains(channel)) {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = false
							});
						} else {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = true,
								GameServers = photonServers.GameServers,
								CommServer = photonServers.CommServer,
								WarnPlayer = !supportedClientVersions.Contains(clientVersion),
								EncryptionInitVector = EncryptionInitVector,
								EncryptionPassPhrase = EncryptionPassPhrase
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
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var clientVersion = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), clientVersion);

					using (var outputStream = new MemoryStream()) {
						if (supportedClientVersions.Contains(clientVersion)) {
							ApplicationConfigurationViewProxy.Serialize(outputStream, applicationConfiguration);
						}

						return isEncrypted
							? CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector)
							: outputStream.ToArray();
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
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var clientType = EnumProxy<DefinitionType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), clientVersion, clientType);

					using (var outputStream = new MemoryStream()) {
						if (supportedClientVersions.Contains(clientVersion)) {
							ListProxy<MapView>.Serialize(outputStream, mapData, MapViewProxy.Serialize);
						}

						return isEncrypted
							? CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector)
							: outputStream.ToArray();
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
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var clientType = EnumProxy<DefinitionType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), clientVersion, clientType);

					using (var outputStream = new MemoryStream()) {
						if (supportedClientVersions.Contains(clientVersion)) {
							ListProxy<ParadiseMapView>.Serialize(outputStream, customMapData, ParadiseMapViewProxy.Serialize);
						}

						return isEncrypted
							? CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector)
							: outputStream.ToArray();
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
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var scoringView = MatchStatsProxy.Deserialize(bytes);
					var serverAuthentication = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), clientVersion, scoringView, serverAuthentication);

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
