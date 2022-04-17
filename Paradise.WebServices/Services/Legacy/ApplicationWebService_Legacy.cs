using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using Paradise.Core.Serialization.Legacy;
using Paradise.DataCenter.Common.Entities;
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
	public class ApplicationWebService_Legacy : WebServiceBase, IApplicationWebServiceContract_Legacy {
		public override string ServiceName => "ApplicationWebService";
		public override string ServiceVersion => "1.0.1";
		protected override Type ServiceInterface => typeof(IApplicationWebServiceContract_Legacy);

		private static string[] supportedClientVersions = { "4.3.9" };

		private ApplicationConfigurationView gameConfiguration;
		private List<MapView> mapData;
		private AuthenticateApplicationView defaultAppAuthentication;

		public ApplicationWebService_Legacy(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() {
			try {
				gameConfiguration = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "ApplicationConfiguration.json")));
				defaultAppAuthentication = JsonConvert.DeserializeObject<AuthenticateApplicationView>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "ApplicationData.json")));
				mapData = JsonConvert.DeserializeObject<List<MapView>>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Maps.json")));
			} catch (Exception e) {
				Log.Error($"Failed to load {ServiceName} data: {e.Message}");
			}
		}

		public byte[] AuthenticateApplication(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clientVersion = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var publicKey = StringProxy.Deserialize(bytes);

					DebugEndpoint(clientVersion, channel, publicKey);

					using (var outputStream = new MemoryStream()) {
						if (channel != ChannelType.WindowsStandalone) {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = false
							});
						} else {
							AuthenticateApplicationViewProxy.Serialize(outputStream, new AuthenticateApplicationView {
								IsEnabled = true,
								GameServers = defaultAppAuthentication.GameServers,
								CommServer = defaultAppAuthentication.CommServer,
								WarnPlayer = !supportedClientVersions.Contains(clientVersion),
								EncryptionInitVector = defaultAppAuthentication.EncryptionInitVector,
								EncryptionPassPhrase = defaultAppAuthentication.EncryptionPassPhrase
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

		public byte[] GetItemAssetBundles(byte[] data) {
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

		public byte[] GetLiveFeed(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						ListProxy<LiveFeedView>.Serialize(outputStream, new List<LiveFeedView> {
							new LiveFeedView {
								Date = DateTime.Now,
								LivedFeedId = 1,
								Description = "Per isst kacke",
								Priority = 0,
								Url = "https://repo.festival.tf"
							},
							new LiveFeedView {
								Date = DateTime.Now,
								LivedFeedId = 2,
								Description = "Per isst Kacke",
								Priority = 1,
								Url = "https://repo.festival.tf"
							}
						}, LiveFeedViewProxy.Serialize);

						//return outputStream.ToArray();
						return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=", "aaaabbbbccccdddd");
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
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						ListProxy<MapView>.Serialize(outputStream, new List<MapView> {
							new MapView {
								Description = "test",
								DisplayName = "Monkey Island",
								FileName = "Map-01-60fdc2144516e36004e7b7cb0daeda93-HD.unity3d",
								SceneName = "LevelMonkeyIsland",
								IsBlueBox = false,
								MapId = 1
							},
							new MapView {
								Description = "test",
								DisplayName = "Lost Paradise 2",
								FileName = "Map-02-a17ce7bfb24073fb195a79d2bf82f9e0-HD.unity3d",
								SceneName = "LevelLostParadise2",
								IsBlueBox = false,
								MapId = 2
							},
							new MapView {
								Description = "test",
								DisplayName = "The Warehouse",
								FileName = "Map-03-80970d51c4d9bc688b8cd2ea756d73ce-HD.unity3d",
								SceneName = "LevelTheWarehouse",
								IsBlueBox = false,
								MapId = 3
							},
							new MapView {
								Description = "test",
								DisplayName = "TempleOfTheRaven",
								FileName = "Map-04-e7992ea16dd9e2004e453168308cf0d1-HD.unity3d",
								SceneName = "LevelTempleOfTheRaven",
								IsBlueBox = false,
								MapId = 4
							},
							new MapView {
								Description = "test",
								DisplayName = "Spaceport Alpha",
								FileName = "Map-10-52d0276860e8f98015e6a6e7fd9329dd-HD.unity3d",
								SceneName = "LevelSpaceportAlpha",
								IsBlueBox = false,
								MapId = 10
							}
						}, MapViewProxy.Serialize);

						//return outputStream.ToArray();
						return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=", "aaaabbbbccccdddd");
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] GetMyIP(byte[] data) {
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

		public byte[] GetPhotonServerName(byte[] data) {
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

		public byte[] GetPhotonServers(byte[] data) {
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

		public byte[] IsAlive(byte[] data) {
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

		public byte[] RecordException(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						//throw new NotImplementedException();

						return outputStream.ToArray();
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

						return outputStream.ToArray();
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

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] RegisterClientApplication(byte[] data) {
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

		public byte[] ReportBug(byte[] data) {
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

		public byte[] SetLevelVersion(byte[] data) {
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