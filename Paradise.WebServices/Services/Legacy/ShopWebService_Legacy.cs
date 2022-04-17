using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using Paradise.Core.Serialization.Legacy;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Paradise.Util.Ciphers;
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
	public class ShopWebService_Legacy : WebServiceBase, IShopWebServiceContract_Legacy {
		public override string ServiceName => "ShopWebService";
		public override string ServiceVersion => "1.0.1";
		protected override Type ServiceInterface => typeof(IShopWebServiceContract_Legacy);

		private UberStrikeItemShopClientView shopData;

		public ShopWebService_Legacy(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() {
			try {
				shopData = JsonConvert.DeserializeObject<UberStrikeItemShopClientView>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Shop_4.3.9.json")));
			} catch (Exception e) {
				Log.Error($"Failed to load {ServiceName} data: {e.Message}");
			}
		}

		public byte[] BuyiPadBundle(byte[] data) {
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

		public byte[] BuyiPhoneBundle(byte[] data) {
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

		public byte[] BuyItem(byte[] data) {
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

		public byte[] BuyMasBundle(byte[] data) {
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

		public byte[] BuyPack(byte[] data) {
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

		public byte[] GetAllLuckyDraws_1(byte[] data) {
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

		public byte[] GetAllLuckyDraws_2(byte[] data) {
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

		public byte[] GetAllMysteryBoxs_1(byte[] data) {
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

		public byte[] GetAllMysteryBoxs_2(byte[] data) {
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

		public byte[] GetBundles(byte[] data) {
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

		public byte[] GetLuckyDraw(byte[] data) {
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

		public byte[] GetMysteryBox(byte[] data) {
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

		public byte[] GetShop(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						UberStrikeItemShopClientViewProxy.Serialize(outputStream, new UberStrikeItemShopClientView {
							FunctionalItems = new List<UberStrikeItemFunctionalView> {
								new UberStrikeItemFunctionalView { 
									ID = 1094,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
								}
							},
							GearItems = new List<UberStrikeItemGearView> {
								new UberStrikeItemGearView
								{
									ID = 1084,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
								},
								new UberStrikeItemGearView
								{
									ID = 1086,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
								},
								new UberStrikeItemGearView
								{
									ID = 1087,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
								},
								new UberStrikeItemGearView
								{
									ID = 1088,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
								},
								new UberStrikeItemGearView
								{
									ID = 1089,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
								}
							},
							QuickItems = new List<UberStrikeItemQuickView> { },
							WeaponItems = new List<UberStrikeItemWeaponView> {
								new UberStrikeItemWeaponView {
									Name = "Splatbat",
									PrefabName = "TheSplatbat",
									Description = "Tier 1 Mace",
									ID = 1000,
									DamageKnockback = 1000,
									DamagePerProjectile = 99,
									AccuracySpread = 0,
									RecoilKickback = 0,
									StartAmmo = 0,
									MaxAmmo = 0,
									MissileTimeToDetonate = 0,
									MissileForceImpulse = 0,
									MissileBounciness = 0,
									RateOfFire = 500,
									SplashRadius = 100,
									ProjectilesPerShot = 1,
									ProjectileSpeed = 0,
									RecoilMovement = 0,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponMelee
								},
								new UberStrikeItemWeaponView
								{
									ID = 1001,
									DamageKnockback = 80,
									DamagePerProjectile = 24,
									AccuracySpread = 3,
									RecoilKickback = 8,
									StartAmmo = 25,
									MaxAmmo = 50,
									MissileTimeToDetonate = 0,
									MissileForceImpulse = 0,
									MissileBounciness = 0,
									RateOfFire = 200,
									SplashRadius = 100,
									ProjectilesPerShot = 1,
									ProjectileSpeed = 0,
									RecoilMovement = 8,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponHandgun
								},
								new UberStrikeItemWeaponView
								{
									ID = 1002,
									DamageKnockback = 50,
									DamagePerProjectile = 13,
									AccuracySpread = 3,
									RecoilKickback = 4,
									StartAmmo = 100,
									MaxAmmo = 300,
									MissileTimeToDetonate = 0,
									MissileForceImpulse = 0,
									MissileBounciness = 0,
									RateOfFire = 125,
									SplashRadius = 100,
									ProjectilesPerShot = 1,
									ProjectileSpeed = 0,
									RecoilMovement = 5,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponMachinegun
								},
								new UberStrikeItemWeaponView
								{
									ID = 1003,
									DamageKnockback = 160,
									DamagePerProjectile = 9,
									AccuracySpread = 8,
									RecoilKickback = 15,
									StartAmmo = 20,
									MaxAmmo = 50,
									MissileTimeToDetonate = 0,
									MissileForceImpulse = 0,
									MissileBounciness = 0,
									RateOfFire = 1000,
									SplashRadius = 100,
									ProjectilesPerShot = 11,
									ProjectileSpeed = 0,
									RecoilMovement = 10,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponShotgun
								},
								new UberStrikeItemWeaponView
								{
									ID = 1004,
									DamageKnockback = 150,
									DamagePerProjectile = 70,
									AccuracySpread = 0,
									RecoilKickback = 12,
									StartAmmo = 20,
									MaxAmmo = 50,
									MissileTimeToDetonate = 0,
									MissileForceImpulse = 0,
									MissileBounciness = 0,
									RateOfFire = 1500,
									SplashRadius = 100,
									ProjectilesPerShot = 1,
									ProjectileSpeed = 0,
									RecoilMovement = 15,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponSniperRifle
								},
								new UberStrikeItemWeaponView
								{
									ID = 1006,
									DamageKnockback = 150,
									DamagePerProjectile = 16,
									AccuracySpread = 0,
									RecoilKickback = 0,
									StartAmmo = 60,
									MaxAmmo = 200,
									MissileTimeToDetonate = 5000,
									MissileForceImpulse = 0,
									MissileBounciness = 80,
									RateOfFire = 90,
									SplashRadius = 80,
									ProjectilesPerShot = 1,
									ProjectileSpeed = 70,
									RecoilMovement = 0,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponSplattergun
								},
								new UberStrikeItemWeaponView
								{
									ID = 1007,
									DamageKnockback = 450,
									DamagePerProjectile = 70,
									AccuracySpread = 0,
									RecoilKickback = 15,
									StartAmmo = 15,
									MaxAmmo = 30,
									MissileTimeToDetonate = 1250,
									MissileForceImpulse = 0,
									MissileBounciness = 0,
									RateOfFire = 1000,
									SplashRadius = 400,
									ProjectilesPerShot = 1,
									ProjectileSpeed = 20,
									RecoilMovement = 9,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponLauncher
								},
								new UberStrikeItemWeaponView
								{
									ID = 1005,
									DamageKnockback = 600,
									DamagePerProjectile = 65,
									AccuracySpread = 0,
									RecoilKickback = 12,
									StartAmmo = 10,
									MaxAmmo = 25,
									MissileTimeToDetonate = 5000,
									MissileForceImpulse = 0,
									MissileBounciness = 0,
									RateOfFire = 1000,
									SplashRadius = 250,
									ProjectilesPerShot = 1,
									ProjectileSpeed = 50,
									RecoilMovement = 32,
									Prices = new List<ItemPrice> {
										new ItemPrice {
											Amount = 0,
											Currency = UberStrikeCurrencyType.Points,
											Duration = BuyingDurationType.Permanent
										}
									},
									ItemClass = Core.Types.UberstrikeItemClass.WeaponCannon
								}
							}
						});

						//return outputStream.ToArray();
						return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=", "aaaabbbbccccdddd");
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] RollLuckyDraw(byte[] data) {
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

		public byte[] RollMysteryBox(byte[] data) {
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

		public byte[] UseConsumableItem(byte[] data) {
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