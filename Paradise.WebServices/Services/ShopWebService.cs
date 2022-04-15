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
using System.Text;

namespace Paradise.WebServices.Services {
	public class ShopWebService : WebServiceBase, IShopWebServiceContract {
		protected override string ServiceName => "ShopWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IShopWebServiceContract);

		private UberStrikeItemShopClientView shopData;
		private List<BundleView> bundleData;

		public ShopWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() {
			try {
				shopData = JsonConvert.DeserializeObject<UberStrikeItemShopClientView>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Shop.json")));
				bundleData = JsonConvert.DeserializeObject<List<BundleView>>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Bundles.json")));
			} catch (Exception e) {
				Log.Error($"Failed to load {ServiceName} data: {e.Message}");
			}
		}

		/// <summary>
		/// Attempts to buy a credits bundle
		/// </summary>
		/// <remarks>
		/// As the game client appears to process microtransactions via Steam, this method is unused
		/// </remarks>
		public byte[] BuyBundle(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var bundleId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var hashedReceipt = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken, bundleId, channel, hashedReceipt);

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

		/// <summary>
		/// Attempts to buy a bundle via Steam
		/// </summary>
		/// <remarks>
		/// The Paradise client patches bypass Steam microtransactions, so that buying bundles is entirely handled by Paradise WebServices
		/// </remarks>
		public byte[] BuyBundleSteam(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var bundleId = Int32Proxy.Deserialize(bytes);
					var steamId = StringProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(bundleId, steamId, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);
						var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

						if (steamMember != null && publicProfile != null) {
							var bundle = bundleData.Find(_ => _.Id == bundleId);

							if (bundle != null) {
								var memberWallet = DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (memberWallet != null) {
									var transactionKey = new byte[32];
									new Random((int)DateTime.UtcNow.Ticks).NextBytes(transactionKey);

									var builder = new StringBuilder(64);
									for (int i = 0; i < transactionKey.Length; i++) {
										Log.Debug(i.ToString());
										builder.Append(transactionKey[i].ToString("x2"));
									}

									DatabaseManager.CurrencyDeposits.Insert(new CurrencyDepositView {
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

									DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
									DatabaseManager.MemberWallets.Insert(memberWallet);

									BooleanProxy.Serialize(outputStream, true);
									return outputStream.ToArray();
								}
							}
						}
						BooleanProxy.Serialize(outputStream, false);


						return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var itemId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var currencyType = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
					var durationType = EnumProxy<BuyingDurationType>.Deserialize(bytes);
					var itemType = EnumProxy<UberstrikeItemType>.Deserialize(bytes);
					var marketLocation = EnumProxy<BuyingLocationType>.Deserialize(bytes);
					var recommendationType = EnumProxy<BuyingRecommendationType>.Deserialize(bytes);

					DebugEndpoint(itemId, authToken, currencyType, durationType, itemType, marketLocation, recommendationType);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);
						var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

						if (steamMember == null || publicProfile == null) {
							Int32Proxy.Serialize(outputStream, (int)BuyItemResult.InvalidMember);
							return outputStream.ToArray();
						}

						var memberWallet = DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == publicProfile.Cmid);
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
							return outputStream.ToArray();
						}

						if (!item.IsForSale) {
							Int32Proxy.Serialize(outputStream, (int)BuyItemResult.IsNotForSale);
							return outputStream.ToArray();
						}

						if (DatabaseManager.PlayerInventoryItems.FindOne(_ => _.Cmid == steamMember.Cmid && _.ItemId == itemId && (_.ExpirationDate > DateTime.Now || _.ExpirationDate == null)) != null) {
							Int32Proxy.Serialize(outputStream, (int)BuyItemResult.AlreadyInInventory);
							return outputStream.ToArray();
						}

						//if (weaponItem.LevelLock) {
						//	TODO: Implement level lock
						//}

						if (currencyType == UberStrikeCurrencyType.Credits) {
							var price = item.Prices.ToList().Find(_ => _.Currency == UberStrikeCurrencyType.Credits);
							if (price != null) {
								if (memberWallet.Credits < price.Price) {
									Int32Proxy.Serialize(outputStream, (int)BuyItemResult.NotEnoughCurrency);
									return outputStream.ToArray();
								} else {
									memberWallet.Credits -= price.Price;

									DatabaseManager.ItemTransactions.Insert(new ItemTransactionView {
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
									return outputStream.ToArray();
								} else {
									memberWallet.Points -= price.Price;

									DatabaseManager.ItemTransactions.Insert(new ItemTransactionView {
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

						DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
						DatabaseManager.MemberWallets.Insert(memberWallet);

						DatabaseManager.PlayerInventoryItems.Insert(new ItemInventoryView {
							Cmid = publicProfile.Cmid,
							ItemId = itemId,
							AmountRemaining = -1
						});

						Int32Proxy.Serialize(outputStream, (int)BuyItemResult.OK);

						return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var itemId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var packType = EnumProxy<PackType>.Deserialize(bytes);
					var currencyType = EnumProxy<UberStrikeCurrencyType>.Deserialize(bytes);
					var itemType = EnumProxy<UberstrikeItemType>.Deserialize(bytes);
					var marketLocation = EnumProxy<BuyingLocationType>.Deserialize(bytes);
					var recommendationType = EnumProxy<BuyingRecommendationType>.Deserialize(bytes);

					DebugEndpoint(itemId, authToken, packType, currencyType, itemType, marketLocation, recommendationType);

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

		/// <summary>
		/// Completes a bundle purchase via Steam
		/// </summary>
		public byte[] FinishBuyBundleSteam(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var orderId = StringProxy.Deserialize(bytes);

					DebugEndpoint(orderId);

					using (var outputStream = new MemoryStream()) {
						BooleanProxy.Serialize(outputStream, true);

						return outputStream.ToArray();
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
				using (var outputStream = new MemoryStream()) {
					DebugEndpoint();
					throw new NotImplementedException();

					return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var bundleCategoryType = EnumProxy<BundleCategoryType>.Deserialize(bytes);

					DebugEndpoint(bundleCategoryType);

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

		/// <summary>
		/// Gets a list of current Mystery Boxes. Appears to be unused by the game
		/// </summary>
		public byte[] GetAllMysteryBoxs_1(byte[] data) {
			try {
				using (var outputStream = new MemoryStream()) {
					DebugEndpoint();
					throw new NotImplementedException();

					return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var bundleCategoryType = EnumProxy<BundleCategoryType>.Deserialize(bytes);

					DebugEndpoint(bundleCategoryType);

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

		/// <summary>
		/// Gets a list of available credit bundles
		/// </summary>
		public byte[] GetBundles(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var channelType = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(channelType);

					using (var outputStream = new MemoryStream()) {
						ListProxy<BundleView>.Serialize(outputStream, bundleData, BundleViewProxy.Serialize);

						return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var luckyDrawId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(luckyDrawId);

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

		/// <summary>
		/// Gets a specific Mystery Box by ID. Appears to be unused by the game
		/// </summary>
		public byte[] GetMysteryBox(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var mysteryBoxId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(mysteryBoxId);

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

		/// <summary>
		/// Gets the current shop data
		/// </summary>
		public byte[] GetShop(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						UberStrikeItemShopClientViewProxy.Serialize(outputStream, shopData);

						return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var luckDrawId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(authToken, luckDrawId, channel);

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

		/// <summary>
		/// Opens a Mystery Box. Appears to be unused by the game
		/// </summary>
		public byte[] RollMysteryBox(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var mysteryBoxId = Int32Proxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);

					DebugEndpoint(authToken, mysteryBoxId, channel);

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

		/// <summary>
		/// Reduces the owned amount of a specific consumable item
		/// </summary>
		public byte[] UseConsumableItem(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var itemId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, itemId);

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

		/// <summary>
		/// Verifies a certain receipt. Appears to be unused by the game as microtransactions are (usually) handled by Steam
		/// </summary>
		public byte[] VerifyReceipt(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var hashedReceipt = StringProxy.Deserialize(bytes);

					DebugEndpoint(hashedReceipt);

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
