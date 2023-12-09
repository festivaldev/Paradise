using Cmune.DataCenter.Common.Entities;
using log4net;
using Newtonsoft.Json;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Serialization;
using UberStrike.Core.Types;

namespace Paradise.WebServices.Services {
	public class ShopWebService : BaseWebService, IShopWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(ShopWebService));

		public override string ServiceName => "ShopWebService";
		public override string ServiceVersion => ApiVersion.Current;
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
						if (!watchedFiles.Contains(e.Name)) return;

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
		/// <summary>
		/// Attempts to buy a credits bundle
		/// </summary>
		/// <remarks>
		/// As the game client appears to process microtransactions via Steam, this method is unused
		/// </remarks>
		public byte[] BuyBundle(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var bundleId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var hashedReceipt = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, bundleId, channel, hashedReceipt);

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

		/// <summary>
		/// Attempts to buy a bundle via Steam
		/// </summary>
		/// <remarks>
		/// The Paradise client patches bypass Steam microtransactions, so that buying bundles is entirely handled by Paradise WebServices
		/// </remarks>
		public byte[] BuyBundleSteam(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var bundleId = Int32Proxy.Deserialize(bytes);
					var steamId = StringProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), bundleId, steamId, authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (steamMember != null && publicProfile != null) {
									var bundle = bundleData.Find(_ => _.Id == bundleId);

									if (bundle != null) {
										var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

										if (memberWallet != null) {
											var transactionKey = new byte[32];
											new Random((int)DateTime.UtcNow.Ticks).NextBytes(transactionKey);

											var builder = new StringBuilder(64);
											for (int i = 0; i < transactionKey.Length; i++) {
												builder.Append(transactionKey[i].ToString("x2"));
											}

											DatabaseClient.CurrencyDeposits.Insert(new CurrencyDepositView {
												BundleId = bundle.Id,
												BundleName = bundle.Name,
												ChannelId = ChannelType.Steam,
												Cmid = publicProfile.Cmid,
												Credits = bundle.Credits,
												CreditsDepositId = new Random((int)DateTime.UtcNow.Ticks).Next(1, int.MaxValue),
												CurrencyLabel = "$",
												DepositDate = DateTime.UtcNow,
												PaymentProviderId = PaymentProviderType.Cmune,
												Points = bundle.Points,
												TransactionKey = builder.ToString(),
												UsdAmount = bundle.USDPrice
											});

											memberWallet.Credits += bundle.Credits;
											memberWallet.Points += bundle.Points;

											DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
											DatabaseClient.MemberWallets.Insert(memberWallet);

											BooleanProxy.Serialize(outputStream, true);
											return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
										}
									}
								}
							}
						}

						BooleanProxy.Serialize(outputStream, false);

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
		/// Attempts to buy items (weapons, gear etc) in the shop
		/// </summary>
		public byte[] BuyItem(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var itemId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var currencyType = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
					var durationType = EnumProxy<BuyingDurationType>.Deserialize(bytes);
					var itemType = EnumProxy<UberstrikeItemType>.Deserialize(bytes);
					var marketLocation = EnumProxy<BuyingLocationType>.Deserialize(bytes);
					var recommendationType = EnumProxy<BuyingRecommendationType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), itemId, authToken, currencyType, durationType, itemType, marketLocation, recommendationType);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
								var playerStatistics = DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (steamMember == null || publicProfile == null) {
									Int32Proxy.Serialize(outputStream, (int)BuyItemResult.InvalidMember);
									return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
								}

								var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == publicProfile.Cmid);
								BaseUberStrikeItemView item = null;

								switch (itemType) {
									case UberstrikeItemType.Weapon:
										item = shopData.WeaponItems.Find(_ => _.ID == itemId);
										break;
									case UberstrikeItemType.Gear:
										item = shopData.GearItems.Find(_ => _.ID == itemId);
										break;
									case UberstrikeItemType.QuickUse:
										item = shopData.QuickItems.Find(_ => _.ID == itemId);
										break;
									case UberstrikeItemType.Functional:
										item = shopData.FunctionalItems.Find(_ => _.ID == itemId);
										break;
									default: break;
								}

								if (item == null) {
									Int32Proxy.Serialize(outputStream, (int)BuyItemResult.ItemNotFound);
									return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
								}

								if (!item.IsForSale) {
									Int32Proxy.Serialize(outputStream, (int)BuyItemResult.IsNotForSale);
									return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
								}

								if (DatabaseClient.PlayerInventoryItems.FindOne(_ => _.Cmid == steamMember.Cmid && _.ItemId == itemId && (_.ExpirationDate > DateTime.UtcNow || _.ExpirationDate == null)) != null) {
									Int32Proxy.Serialize(outputStream, (int)BuyItemResult.AlreadyInInventory);
									return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
								}

								if (XpPointsUtil.GetLevelForXp(playerStatistics.Xp) < item.LevelLock) {
									Int32Proxy.Serialize(outputStream, (int)BuyItemResult.InvalidLevel);
									return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
								}

								if (currencyType == UberStrikeCurrencyType.Credits) {
									var price = item.Prices.ToList().Find(_ => _.Currency == UberStrikeCurrencyType.Credits);
									if (price != null) {
										if (memberWallet.Credits < price.Price) {
											Int32Proxy.Serialize(outputStream, (int)BuyItemResult.NotEnoughCurrency);
											return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
										} else {
											memberWallet.Credits -= price.Price;

											DatabaseClient.ItemTransactions.Insert(new ItemTransactionView {
												Cmid = publicProfile.Cmid,
												Duration = durationType,
												ItemId = itemId,
												Points = price.Price,
												WithdrawalDate = DateTime.UtcNow,
												WithdrawalId = new Random((int)DateTime.UtcNow.Ticks).Next(1, int.MaxValue)
											});
										}
									}
								} else if (currencyType == UberStrikeCurrencyType.Points) {
									var price = item.Prices.ToList().Find(_ => _.Currency == UberStrikeCurrencyType.Points);
									if (price != null) {
										if (memberWallet.Points < price.Price) {
											Int32Proxy.Serialize(outputStream, (int)BuyItemResult.NotEnoughCurrency);
											return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
										} else {
											memberWallet.Points -= price.Price;

											DatabaseClient.ItemTransactions.Insert(new ItemTransactionView {
												Cmid = publicProfile.Cmid,
												Credits = price.Price,
												Duration = durationType,
												ItemId = itemId,
												WithdrawalDate = DateTime.UtcNow,
												WithdrawalId = new Random((int)DateTime.UtcNow.Ticks).Next(1, int.MaxValue)
											});
										}
									}
								}

								DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
								DatabaseClient.MemberWallets.Insert(memberWallet);

								DateTime? expirationDate = null;

								switch (durationType) {
									case BuyingDurationType.OneDay:
										expirationDate = DateTime.UtcNow.AddDays(1);
										break;
									case BuyingDurationType.SevenDays:
										expirationDate = DateTime.UtcNow.AddDays(7);
										break;
									case BuyingDurationType.ThirtyDays:
										expirationDate = DateTime.UtcNow.AddDays(30);
										break;
									case BuyingDurationType.NinetyDays:
										expirationDate = DateTime.UtcNow.AddDays(90);
										break;
								}

								DatabaseClient.PlayerInventoryItems.Insert(new ItemInventoryView {
									Cmid = publicProfile.Cmid,
									ItemId = itemId,
									AmountRemaining = -1,
									ExpirationDate = expirationDate
								});

								Int32Proxy.Serialize(outputStream, (int)BuyItemResult.OK);
							}
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
		/// Attempts to buy consumables (eg. Spring Grenades) in the shop
		/// </summary>
		public byte[] BuyPack(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var itemId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var packType = EnumProxy<PackType>.Deserialize(bytes);
					var currencyType = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
					var itemType = EnumProxy<UberstrikeItemType>.Deserialize(bytes);
					var marketLocation = EnumProxy<BuyingLocationType>.Deserialize(bytes);
					var recommendationType = EnumProxy<BuyingRecommendationType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), itemId, authToken, packType, currencyType, itemType, marketLocation, recommendationType);

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

		/// <summary>
		/// Completes a bundle purchase via Steam
		/// </summary>
		public byte[] FinishBuyBundleSteam(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var orderId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), orderId);

					using (var outputStream = new MemoryStream()) {
						BooleanProxy.Serialize(outputStream, true);

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
		/// Gets a list of current Lucky Draws. Appears to be unused by the game
		/// </summary>
		public byte[] GetAllLuckyDraws_1(byte[] data) {
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

		/// <summary>
		/// Gets a list of current Lucky Draws of a specified bundle category. Appears to be unused by the game
		/// </summary>
		public byte[] GetAllLuckyDraws_2(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var bundleCategoryType = EnumProxy<BundleCategoryType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), bundleCategoryType);

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

		/// <summary>
		/// Gets a list of current Mystery Boxes. Appears to be unused by the game
		/// </summary>
		public byte[] GetAllMysteryBoxs_1(byte[] data) {
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

		/// <summary>
		/// Gets a list of current Mystery Boxes of a specified bundle category. Appears to be unused by the game
		/// </summary>
		public byte[] GetAllMysteryBoxs_2(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var bundleCategoryType = EnumProxy<BundleCategoryType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), bundleCategoryType);

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

		/// <summary>
		/// Gets a list of available credit bundles
		/// </summary>
		public byte[] GetBundles(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var channelType = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), channelType);

					using (var outputStream = new MemoryStream()) {
						ListProxy<BundleView>.Serialize(outputStream, bundleData, BundleViewProxy.Serialize);

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
		/// Gets a specific Lucky Draw by ID. Appears to be unused by the game
		/// </summary>
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

		/// <summary>
		/// Gets a specific Mystery Box by ID. Appears to be unused by the game
		/// </summary>
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

		/// <summary>
		/// Gets the current shop data
		/// </summary>
		public byte[] GetShop(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

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

		/// <summary>
		/// Rolls a Lucky Draw. Appears to be unused by the game
		/// </summary>
		public byte[] RollLuckyDraw(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var luckDrawId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, luckDrawId, channel);

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

		/// <summary>
		/// Opens a Mystery Box. Appears to be unused by the game
		/// </summary>
		public byte[] RollMysteryBox(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var mysteryBoxId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, mysteryBoxId, channel);

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

		/// <summary>
		/// Reduces the owned amount of a specific consumable item
		/// </summary>
		public byte[] UseConsumableItem(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var itemId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, itemId);

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

		/// <summary>
		/// Verifies a certain receipt. Appears to be unused by the game as microtransactions are (usually) handled by Steam
		/// </summary>
		public byte[] VerifyReceipt(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var hashedReceipt = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), hashedReceipt);

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
