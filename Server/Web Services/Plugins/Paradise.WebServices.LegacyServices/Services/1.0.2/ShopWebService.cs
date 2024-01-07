using Cmune.DataCenter.Common.Entities;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Serialization.Legacy;
using UberStrike.Core.Types;

namespace Paradise.WebServices.LegacyServices._102 {
	public class ShopWebService : BaseWebService, IShopWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(ShopWebService));

		public override string ServiceName => "ShopWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IShopWebServiceContract);

		private UberStrikeItemShopClientView shopData;
		private List<BundleView> bundleData;

		private FileSystemWatcher watcher;
		private static readonly List<string> watchedFiles = new List<string> {
			"Shop.json",
			"Bundles.json",
		};

		public ShopWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			try {
				shopData = JsonConvert.DeserializeObject<UberStrikeItemShopClientView>(File.ReadAllText(Path.Combine(ServiceDataPath, "Shop.json")));
				bundleData = JsonConvert.DeserializeObject<List<BundleView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Bundles.json")));

				watcher = new FileSystemWatcher(ServiceDataPath) {
					NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
				};

				watcher.Changed += (object sender, FileSystemEventArgs e) => {
					try {
						if (!watchedFiles.Contains(e.Name))
							return;

						Task.Run(async () => {
							await Task.Delay(500);
							Log.Info("Reloading Shop service data due to file changes.");

							shopData = JsonConvert.DeserializeObject<UberStrikeItemShopClientView>(File.ReadAllText(Path.Combine(ServiceDataPath, "Shop.json")));
							bundleData = JsonConvert.DeserializeObject<List<BundleView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "Bundles.json")));
						});
					} catch (Exception ex) {
						Log.Error(ex);
					}
				};

				watcher.EnableRaisingEvents = true;
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

		#region IShopWebServiceContract
		public byte[] GetShop(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var applicationVersion = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), applicationVersion);

					using (var outputStream = new MemoryStream()) {
						UberStrikeItemShopClientViewProxy.Serialize(outputStream, shopData);

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

		public byte[] BuyItem(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var itemId = Int32Proxy.Deserialize(bytes);
					var buyerCmid = Int32Proxy.Deserialize(bytes);
					var currencyType = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
					var durationType = EnumProxy<BuyingDurationType>.Deserialize(bytes);
					var itemType = EnumProxy<UberstrikeItemType>.Deserialize(bytes);
					var marketLocation = EnumProxy<BuyingLocationType>.Deserialize(bytes);
					var recommendationType = EnumProxy<BuyingRecommendationType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), itemId, buyerCmid, currencyType, durationType, itemType, marketLocation, recommendationType);

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

		public byte[] BuyPack(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var itemId = Int32Proxy.Deserialize(bytes);
					var buyerCmid = Int32Proxy.Deserialize(bytes);
					var packType = EnumProxy<PackType>.Deserialize(bytes);
					var currencyType = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
					var itemType = EnumProxy<UberstrikeItemType>.Deserialize(bytes);
					var marketLocation = EnumProxy<BuyingLocationType>.Deserialize(bytes);
					var recommendationType = EnumProxy<BuyingRecommendationType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), itemId, buyerCmid, packType, currencyType, itemType, marketLocation, recommendationType);

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

		public byte[] GetBundles(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), channel);

					using (var outputStream = new MemoryStream()) {
						ListProxy<BundleView>.Serialize(outputStream, bundleData, BundleViewProxy.Serialize);

						return isEncrypted
							? CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector)
							: outputStream.ToArray();

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

		public byte[] BuyBundle(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var bundleId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var hashedReceipt = StringProxy.Deserialize(bytes);
					var applicationId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, bundleId, channel, hashedReceipt, applicationId);

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

		public byte[] UseConsumableItem(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var itemId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, itemId);

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

		public byte[] GetAllMysteryBoxs_1(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

					using (var outputStream = new MemoryStream()) {
						ListProxy<MysteryBoxUnityView>.Serialize(outputStream, new List<MysteryBoxUnityView> { }, MysteryBoxUnityViewProxy.Serialize);

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

		public byte[] GetAllMysteryBoxs_2(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var bundleCategoryType = EnumProxy<BundleCategoryType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), bundleCategoryType);

					using (var outputStream = new MemoryStream()) {
						ListProxy<MysteryBoxUnityView>.Serialize(outputStream, new List<MysteryBoxUnityView> { }, MysteryBoxUnityViewProxy.Serialize);

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

		public byte[] GetMysteryBox(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var mysteryBoxId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), mysteryBoxId);

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

		public byte[] RollMysteryBox(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var mysteryBoxId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, mysteryBoxId, channel);

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

		public byte[] GetAllLuckyDraws_1(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

					using (var outputStream = new MemoryStream()) {
						ListProxy<LuckyDrawUnityView>.Serialize(outputStream, new List<LuckyDrawUnityView> { }, LuckyDrawUnityViewProxy.Serialize);

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

		public byte[] GetAllLuckyDraws_2(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var bundleCategoryType = EnumProxy<BundleCategoryType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), bundleCategoryType);

					using (var outputStream = new MemoryStream()) {
						ListProxy<LuckyDrawUnityView>.Serialize(outputStream, new List<LuckyDrawUnityView> { }, LuckyDrawUnityViewProxy.Serialize);

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

		public byte[] GetLuckyDraw(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var luckyDrawId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), luckyDrawId);

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

		public byte[] RollLuckyDraw(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var luckDrawId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, luckDrawId, channel);

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
