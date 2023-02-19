using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using Paradise.Core.Serialization.Legacy;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.WebServices.LegacyWebServices {
	public class ApplicationWebService : BaseWebService, IApplicationWebServiceContract {
		public override string ServiceName => "ApplicationWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IApplicationWebServiceContract);

		private static readonly string[] supportedClientVersions = { "4.3.10" };

		private ApplicationConfigurationView applicationConfiguration;
		private AuthenticateApplicationView photonServers;
		private List<MapView> mapData;

		public ApplicationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			try {
				applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "Legacy", "ApplicationWebService", "ApplicationConfiguration.json")));
				photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "Legacy", "ApplicationWebService", "PhotonServers.json")));
				//mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "Maps.json")));
				//customMapData = JsonConvert.DeserializeObject<List<UberstrikeCustomMapView>>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "CustomMaps.json")));

				//watcher = new FileSystemWatcher(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService"));
				//watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
				//watcher.Changed += delegate (object sender, FileSystemEventArgs e) {
				//	applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "ApplicationConfiguration.json")));
				//	photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "PhotonServers.json")));
				//	mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "Maps.json")));
				//	customMapData = JsonConvert.DeserializeObject<List<UberstrikeCustomMapView>>(File.ReadAllText(Path.Combine(CurrentDirectory, "ServiceData", "ApplicationWebService", "CustomMaps.json")));
				//};
				//watcher.EnableRaisingEvents = true;

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

		}



		public byte[] GetMyIP(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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
								WarnPlayer = !supportedClientVersions.Contains(clientVersion),
								EncryptionInitVector = photonServers.EncryptionInitVector,
								EncryptionPassPhrase = photonServers.EncryptionPassPhrase
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

		public byte[] RecordException(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] RecordExceptionUnencrypted(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] RecordTutorialStep(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] ReportBug(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] GetLiveFeed(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] GetMaps(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var appVersion = StringProxy.Deserialize(bytes);
					var locale = EnumProxy<LocaleType>.Deserialize(bytes);
					var definition = EnumProxy<DefinitionType>.Deserialize(bytes);

					DebugEndpoint(appVersion, locale, definition);

					using (var outputStream = new MemoryStream()) {
						ListProxy<MapView>.Serialize(outputStream, new List<MapView>(), MapViewProxy.Serialize);

						//return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] GetItemAssetBundles(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] SetLevelVersion(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] GetPhotonServerName(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] IsAlive(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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
