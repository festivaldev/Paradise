using Cmune.DataCenter.Common.Entities;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Serialization.Legacy;
using UberStrike.Core.Types;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.LegacyServices._102 {
	public class ApplicationWebService : BaseWebService, IApplicationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(ApplicationWebService));

		public override string ServiceName => "ApplicationWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IApplicationWebServiceContract);

		private static readonly string[] supportedClientVersions = { "4.3.10" };
		private static readonly ChannelType[] supportedClientChannels = { ChannelType.WindowsStandalone };

		//private ApplicationConfigurationView applicationConfiguration;
		private AuthenticateApplicationView photonServers;
		private List<MapView> mapData;
		//private List<ParadiseMapView> customMapData;

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
				//applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "ApplicationConfiguration.json")));
				photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "PhotonServers.json")));
				mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Maps.json")));
				//	customMapData = JsonConvert.DeserializeObject<List<UberstrikeCustomMapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "CustomMaps.json")));

				watcher = new FileSystemWatcher(ServiceDataPath) {
					NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
				};

				watcher.Changed += (object sender, FileSystemEventArgs e) => {
					if (!watchedFiles.Contains(e.Name)) return;

					Task.Run(async () => {
						await Task.Delay(500);
						Log.Info("Reloading Application service data due to file changes.");

						//applicationConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "ApplicationConfiguration.json")));
						photonServers = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(ServiceDataPath, "PhotonServers.json")));
						mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Maps.json")));
						//customMapData = JsonConvert.DeserializeObject<List<ParadiseMapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "CustomMaps.json")));
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
		public byte[] GetPhotonServers(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var applicationView = ApplicationViewProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), applicationView);

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

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, buildType, channelType, buildNumber, logString, stackTrace, exceptionData);

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
					var buildType = EnumProxy<BuildType>.Deserialize(bytes);
					var channelType = EnumProxy<ChannelType>.Deserialize(bytes);
					var buildNumber = StringProxy.Deserialize(bytes);
					var errorType = StringProxy.Deserialize(bytes);
					var errorMessage = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), buildType, channelType, buildNumber, errorType, errorMessage);

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
					var cmid = Int32Proxy.Deserialize(bytes);
					var step = EnumProxy<TutorialStepType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, step);

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(cmid));

						if (userAccount != null) {
							userAccount.TutorialStep = step;

							DatabaseClient.UserAccounts.DeleteMany(_ => _.Cmid.Equals(userAccount.Cmid));
							DatabaseClient.UserAccounts.Insert(userAccount);
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

		public byte[] ReportBug(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var bugView = BugViewProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), bugView);

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
					var appVersion = StringProxy.Deserialize(bytes);
					var locale = EnumProxy<LocaleType>.Deserialize(bytes);
					var definition = EnumProxy<DefinitionType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), appVersion, locale, definition);

					using (var outputStream = new MemoryStream()) {
						if (supportedClientVersions.Contains(appVersion)) {
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

		public byte[] GetItemAssetBundles(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var appVersion = StringProxy.Deserialize(bytes);
					var definition = EnumProxy<DefinitionType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), appVersion, definition);

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
					var id = Int32Proxy.Deserialize(bytes);
					var version = Int32Proxy.Deserialize(bytes);
					var md5Hash = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), id, version, md5Hash);

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
					var applicationVersion = StringProxy.Deserialize(bytes);
					var ipAddress = StringProxy.Deserialize(bytes);
					var port = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), applicationVersion, ipAddress, port);

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
