using log4net;
using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using Paradise.Core.Serialization.Legacy;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;

namespace Paradise.WebServices.LegacyServices._102 {
	public class ApplicationWebService : BaseWebService, IApplicationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(ApplicationWebService));

		public override string ServiceName => "ApplicationWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IApplicationWebServiceContract);

		private static readonly string[] supportedClientVersions = { "4.3.10" };
		private static readonly ChannelType[] supportedClientChannels = { ChannelType.WindowsStandalone };

		private ApplicationConfigurationView applicationConfiguration;
		private AuthenticateApplicationView photonServers;

		public ApplicationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			try {
				applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "ApplicationConfiguration.json")));
				photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "PhotonServers.json")));
				//	mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Maps.json")));
				//	customMapData = JsonConvert.DeserializeObject<List<UberstrikeCustomMapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "CustomMaps.json")));

				//	watcher = new FileSystemWatcher(ServiceDataPath) {
				//		NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
				//	};

				//	watcher.Changed += (object sender, FileSystemEventArgs e) => {
				//		if (!watchedFiles.Contains(e.Name)) return;

				//		applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "ApplicationConfiguration.json")));
				//		photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "PhotonServers.json")));
				//		mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Maps.json")));
				//		customMapData = JsonConvert.DeserializeObject<List<UberstrikeCustomMapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "CustomMaps.json")));
				//	};

				//	watcher.EnableRaisingEvents = true;

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

		#region IApplicationWebServiceContract
		public byte[] GetPhotonServers(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] GetMyIP(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] AuthenticateApplication(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var version = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var publicKey = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), version, channel, publicKey);

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
								WarnPlayer = !supportedClientVersions.Contains(version),
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

		public byte[] RecordException(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var buildType = EnumProxy<BuildType>.Deserialize(bytes);
					var channelType = EnumProxy<ChannelType>.Deserialize(bytes);
					var buildNumber = StringProxy.Deserialize(bytes);
					var logString = StringProxy.Deserialize(bytes);
					var stackTrace = StringProxy.Deserialize(bytes);
					var exceptionData = StringProxy.Deserialize(bytes);

					//DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, buildType, channelType, buildNumber, logString, stackTrace, exceptionData);

					using (var outputStream = new MemoryStream()) {
						//throw new NotImplementedException();
						return outputStream.ToArray();

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

		public byte[] RecordExceptionUnencrypted(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] RecordTutorialStep(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] ReportBug(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] GetLiveFeed(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] GetMaps(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

					using (var outputStream = new MemoryStream()) {
						ListProxy<MapView>.Serialize(outputStream, new List<MapView>(), MapViewProxy.Serialize);

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

		public byte[] GetItemAssetBundles(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] SetLevelVersion(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] GetPhotonServerName(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		public byte[] IsAlive(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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
