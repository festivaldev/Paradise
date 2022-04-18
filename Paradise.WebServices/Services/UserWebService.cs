using Paradise.Core.Serialization;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Paradise.WebServices.Services {
	public class UserWebService : WebServiceBase, IUserWebServiceContract {
		public override string ServiceName => "UserWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IUserWebServiceContract);

		public UserWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public UserWebService(BasicHttpBinding binding, ParadiseSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }

		/// <summary>
		/// Adds an item transaction to a player's history
		/// </summary>
		public byte[] AddItemTransaction(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var itemTransaction = ItemTransactionViewProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(itemTransaction, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							DatabaseManager.ItemTransactions.Insert(itemTransaction);

							BooleanProxy.Serialize(outputStream, true);
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
		/// Changes a member's name for using the Name Change item
		/// </summary>
		public byte[] ChangeMemberName(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var name = StringProxy.Deserialize(bytes);
					var locale = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken, name, locale, machineId);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var existingName = DatabaseManager.PublicProfiles.FindOne(_ => _.Name == name);
							var nameChangeItem = DatabaseManager.PlayerInventoryItems.FindOne(_ => _.Cmid == publicProfile.Cmid && _.ItemId == (int)UberstrikeInventoryItem.NameChange);

							if (nameChangeItem == null) {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.NameChangeNotInInventory);
							} else if (existingName != null && existingName.Cmid != publicProfile.Cmid) {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.DuplicateName);
							} else {
								publicProfile.Name = name;
								DatabaseManager.PublicProfiles.Update(publicProfile);

								DatabaseManager.PlayerInventoryItems.DeleteMany(_ => _.Cmid == publicProfile.Cmid && _.ItemId == (int)UberstrikeInventoryItem.NameChange);

								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
							}
						} else {
							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.MemberNotFound);
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
		/// Adds a credit deposit transaction to a player's history
		/// </summary>
		public byte[] DepositCredits(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var depositTransaction = CurrencyDepositViewProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(depositTransaction, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var memberWallet = DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (memberWallet != null) {
								DatabaseManager.CurrencyDeposits.Insert(depositTransaction);

								memberWallet.Credits += depositTransaction.Credits;
								memberWallet.Points += depositTransaction.Points;

								DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
								DatabaseManager.MemberWallets.Insert(memberWallet);

								BooleanProxy.Serialize(outputStream, true);
							}
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
		/// Adds a point deposit transaction to a player's history
		/// </summary>
		public byte[] DepositPoints(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var depositTransaction = PointDepositViewProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(depositTransaction, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var memberWallet = DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (memberWallet != null) {
								DatabaseManager.PointDeposits.Insert(depositTransaction);

								memberWallet.Points += depositTransaction.Points;

								DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
								DatabaseManager.MemberWallets.Insert(memberWallet);

								BooleanProxy.Serialize(outputStream, true);
							}

							BooleanProxy.Serialize(outputStream, true);
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
		/// Generates a list of unique usernames in case the desired username is already in use
		/// </summary>
		public byte[] GenerateNonDuplicatedMemberNames(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var username = StringProxy.Deserialize(bytes);

					DebugEndpoint(username);

					using (var outputStream = new MemoryStream()) {
						ListProxy<string>.Serialize(outputStream, new List<string>(), StringProxy.Serialize);

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Gets a list of historical currency deposits (such as purchasing credit bundles) for the current user
		/// </summary>
		/// <seealso cref="ShopWebService.BuyBundleSteam(byte[])"/>
		public byte[] GetCurrencyDeposits(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, pageIndex, elementPerPage);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var currencyDeposits = DatabaseManager.CurrencyDeposits.Find(_ => _.Cmid == steamMember.Cmid);

							CurrencyDepositsViewModelProxy.Serialize(outputStream, new CurrencyDepositsViewModel {
								CurrencyDeposits = currencyDeposits.Skip((pageIndex - 1) * elementPerPage).Take(elementPerPage).ToList(),
								TotalCount = currencyDeposits.Count()
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
		/// Returns a list of owned items
		/// </summary>
		public byte[] GetInventory(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var playerInventoryItems = DatabaseManager.PlayerInventoryItems.Find(_ => _.Cmid == steamMember.Cmid && (_.ExpirationDate == null || _.ExpirationDate >= DateTime.Now)).ToList();

							ListProxy<ItemInventoryView>.Serialize(outputStream, playerInventoryItems, ItemInventoryViewProxy.Serialize);
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
		/// Gets a list of historical item transactions for the current user
		/// </summary>
		public byte[] GetItemTransactions(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, pageIndex, elementPerPage);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var itemTransactions = DatabaseManager.ItemTransactions.Find(_ => _.Cmid == steamMember.Cmid);

							ItemTransactionsViewModelProxy.Serialize(outputStream, new ItemTransactionsViewModel {
								ItemTransactions = itemTransactions.Skip((pageIndex - 1) * elementPerPage).Take(elementPerPage).ToList(),
								TotalCount = itemTransactions.Count()
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
		/// Gets a players current loadout (or creates a new default loadout if it doesn't exist)
		/// </summary>
		public byte[] GetLoadout(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var playerLoadout = DatabaseManager.PlayerLoadouts.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (playerLoadout == null) {
								playerLoadout = new LoadoutView {
									Cmid = steamMember.Cmid,
									MeleeWeapon = (int)UberstrikeInventoryItem.TheSplatbat,
									Weapon1 = (int)UberstrikeInventoryItem.MachineGun
								};

								DatabaseManager.PlayerLoadouts.Insert(playerLoadout);
							}

							LoadoutViewProxy.Serialize(outputStream, playerLoadout);
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
		/// Gets public information of a player and their statistics
		/// </summary>
		public byte[] GetMember(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var memberWallet = DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);
							var playerStatistics = DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

							UberstrikeUserViewModelProxy.Serialize(outputStream, new UberstrikeUserViewModel {
								CmuneMemberView = new MemberView {
									PublicProfile = publicProfile,
									MemberWallet = memberWallet
								},
								UberstrikeMemberView = new UberstrikeMemberView {
									PlayerStatisticsView = playerStatistics
								}
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
		/// Gets a list of (historical) sessions of a player. Appears to be unused by the game
		/// </summary>
		public byte[] GetMemberListSessionData(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authTokens = ListProxy<string>.Deserialize(bytes, new ListProxy<string>.Deserializer<string>(StringProxy.Deserialize));
					DebugEndpoint(authTokens);

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
		/// Gets the current session data of a player. Appears to be unused by the game
		/// </summary>
		public byte[] GetMemberSessionData(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

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
		/// Gets the current wallet (amount of points and credits) of a player
		/// </summary>
		public byte[] GetMemberWallet(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var memberWallet = DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (memberWallet != null) {
								MemberWalletViewProxy.Serialize(outputStream, memberWallet);
							}
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
		/// Gets a list of historical points deposits for the current user
		/// </summary>
		public byte[] GetPointsDeposits(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, pageIndex, elementPerPage);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var pointDeposits = DatabaseManager.PointDeposits.Find(_ => _.Cmid == steamMember.Cmid);

							PointDepositsViewModelProxy.Serialize(outputStream, new PointDepositsViewModel {
								PointDeposits = pointDeposits.Skip((pageIndex - 1) * elementPerPage).Take(elementPerPage).ToList(),
								TotalCount = pointDeposits.Count()
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
		/// Checks if a username is already in use
		/// </summary>
		public byte[] IsDuplicateMemberName(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var username = StringProxy.Deserialize(bytes);

					DebugEndpoint(username);

					using (var outputStream = new MemoryStream()) {
						BooleanProxy.Serialize(outputStream, DatabaseManager.PublicProfiles.FindOne(_ => _.Name == username) != null);

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Sets the loadout of the current player
		/// </summary>
		public byte[] SetLoadout(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var loadoutView = LoadoutViewProxy.Deserialize(bytes);

					DebugEndpoint(authToken, loadoutView);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var playerLoadout = DatabaseManager.PlayerLoadouts.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (playerLoadout != null) {
								playerLoadout.UpperBody = loadoutView.UpperBody;
								playerLoadout.Weapon1 = loadoutView.Weapon1;
								playerLoadout.Weapon1Mod1 = loadoutView.Weapon1Mod1;
								playerLoadout.Weapon1Mod2 = loadoutView.Weapon1Mod2;
								playerLoadout.Weapon1Mod3 = loadoutView.Weapon1Mod3;
								playerLoadout.Weapon2 = loadoutView.Weapon2;
								playerLoadout.Weapon2Mod1 = loadoutView.Weapon2Mod1;
								playerLoadout.Weapon2Mod2 = loadoutView.Weapon2Mod2;
								playerLoadout.Weapon2Mod3 = loadoutView.Weapon2Mod3;
								playerLoadout.Weapon3 = loadoutView.Weapon3;
								playerLoadout.Weapon3Mod1 = loadoutView.Weapon3Mod1;
								playerLoadout.Weapon3Mod2 = loadoutView.Weapon3Mod2;
								playerLoadout.Weapon3Mod3 = loadoutView.Weapon3Mod3;
								playerLoadout.Type = loadoutView.Type;
								playerLoadout.QuickItem3 = loadoutView.QuickItem3;
								playerLoadout.QuickItem2 = loadoutView.QuickItem2;
								playerLoadout.QuickItem1 = loadoutView.QuickItem1;
								playerLoadout.MeleeWeapon = loadoutView.MeleeWeapon;
								playerLoadout.LowerBody = loadoutView.LowerBody;
								playerLoadout.Head = loadoutView.Head;
								playerLoadout.Gloves = loadoutView.Gloves;
								playerLoadout.FunctionalItem3 = loadoutView.FunctionalItem3;
								playerLoadout.FunctionalItem2 = loadoutView.FunctionalItem2;
								playerLoadout.FunctionalItem1 = loadoutView.FunctionalItem1;
								playerLoadout.Face = loadoutView.Face;
								playerLoadout.Cmid = loadoutView.Cmid;
								playerLoadout.Boots = loadoutView.Boots;
								playerLoadout.Backpack = loadoutView.Backpack;
								playerLoadout.LoadoutId = loadoutView.LoadoutId;
								playerLoadout.Webbing = loadoutView.Webbing; // Holo
								playerLoadout.SkinColor = loadoutView.SkinColor;

								DatabaseManager.PlayerLoadouts.Update(playerLoadout);
							} else {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								return outputStream.ToArray();
							}

							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);

							return outputStream.ToArray();
						} else {
							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.MemberNotFound);
							return outputStream.ToArray();
						}
					}

					throw new NotImplementedException();
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] UpdatePlayerStatistics(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var playerStatistics = PlayerStatisticsViewProxy.Deserialize(bytes);

					DebugEndpoint(authToken, playerStatistics);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var statistics = DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (statistics != null) {
								statistics.Hits = playerStatistics.Hits;
								statistics.Shots = playerStatistics.Shots;
								statistics.Splats = playerStatistics.Splats;
								statistics.Splatted = playerStatistics.Splatted;
								statistics.Headshots = playerStatistics.Headshots;
								statistics.Nutshots = playerStatistics.Nutshots;
								statistics.Xp = playerStatistics.Xp;
								statistics.TimeSpentInGame = playerStatistics.TimeSpentInGame;
								statistics.Level = playerStatistics.Level;

								// Machine Gun
								statistics.WeaponStatistics.MachineGunTotalDamageDone = playerStatistics.WeaponStatistics.MachineGunTotalDamageDone;
								statistics.WeaponStatistics.MachineGunTotalSplats = playerStatistics.WeaponStatistics.MachineGunTotalSplats;
								statistics.WeaponStatistics.MachineGunTotalShotsFired = playerStatistics.WeaponStatistics.MachineGunTotalShotsFired;
								statistics.WeaponStatistics.MachineGunTotalShotsHit = playerStatistics.WeaponStatistics.MachineGunTotalShotsHit;

								// Shotgun
								statistics.WeaponStatistics.ShotgunTotalDamageDone = playerStatistics.WeaponStatistics.ShotgunTotalDamageDone;
								statistics.WeaponStatistics.ShotgunTotalSplats = playerStatistics.WeaponStatistics.ShotgunTotalSplats;
								statistics.WeaponStatistics.ShotgunTotalShotsFired = playerStatistics.WeaponStatistics.ShotgunTotalShotsFired;
								statistics.WeaponStatistics.ShotgunTotalShotsHit = playerStatistics.WeaponStatistics.ShotgunTotalShotsHit;

								// Splattergun
								statistics.WeaponStatistics.SplattergunTotalDamageDone = playerStatistics.WeaponStatistics.SplattergunTotalDamageDone;
								statistics.WeaponStatistics.SplattergunTotalSplats = playerStatistics.WeaponStatistics.SplattergunTotalSplats;
								statistics.WeaponStatistics.SplattergunTotalShotsFired = playerStatistics.WeaponStatistics.SplattergunTotalShotsFired;
								statistics.WeaponStatistics.SplattergunTotalShotsHit = playerStatistics.WeaponStatistics.SplattergunTotalShotsHit;

								// Sniper Rifle
								statistics.WeaponStatistics.SniperTotalDamageDone = playerStatistics.WeaponStatistics.SniperTotalDamageDone;
								statistics.WeaponStatistics.SniperTotalSplats = playerStatistics.WeaponStatistics.SniperTotalSplats;
								statistics.WeaponStatistics.SniperTotalShotsFired = playerStatistics.WeaponStatistics.SniperTotalShotsFired;
								statistics.WeaponStatistics.SniperTotalShotsHit = playerStatistics.WeaponStatistics.SniperTotalShotsHit;

								// Melee Weapons
								statistics.WeaponStatistics.MeleeTotalDamageDone = playerStatistics.WeaponStatistics.MeleeTotalDamageDone;
								statistics.WeaponStatistics.MeleeTotalSplats = playerStatistics.WeaponStatistics.MeleeTotalSplats;
								statistics.WeaponStatistics.MeleeTotalShotsFired = playerStatistics.WeaponStatistics.MeleeTotalShotsFired;
								statistics.WeaponStatistics.MeleeTotalShotsHit = playerStatistics.WeaponStatistics.MeleeTotalShotsHit;

								// Cannon
								statistics.WeaponStatistics.CannonTotalDamageDone = playerStatistics.WeaponStatistics.CannonTotalDamageDone;
								statistics.WeaponStatistics.CannonTotalSplats = playerStatistics.WeaponStatistics.CannonTotalSplats;
								statistics.WeaponStatistics.CannonTotalShotsFired = playerStatistics.WeaponStatistics.CannonTotalShotsFired;
								statistics.WeaponStatistics.CannonTotalShotsHit = playerStatistics.WeaponStatistics.CannonTotalShotsHit;

								// Launcher
								statistics.WeaponStatistics.LauncherTotalDamageDone = playerStatistics.WeaponStatistics.LauncherTotalDamageDone;
								statistics.WeaponStatistics.LauncherTotalSplats = playerStatistics.WeaponStatistics.LauncherTotalSplats;
								statistics.WeaponStatistics.LauncherTotalShotsFired = playerStatistics.WeaponStatistics.LauncherTotalShotsFired;
								statistics.WeaponStatistics.LauncherTotalShotsHit = playerStatistics.WeaponStatistics.LauncherTotalShotsHit;

								statistics.PersonalRecord.MostArmorPickedUp = playerStatistics.PersonalRecord.MostArmorPickedUp;
								statistics.PersonalRecord.MostCannonSplats = playerStatistics.PersonalRecord.MostCannonSplats;
								statistics.PersonalRecord.MostConsecutiveSnipes = playerStatistics.PersonalRecord.MostConsecutiveSnipes;
								statistics.PersonalRecord.MostDamageDealt = playerStatistics.PersonalRecord.MostDamageDealt;
								statistics.PersonalRecord.MostDamageReceived = playerStatistics.PersonalRecord.MostDamageReceived;
								statistics.PersonalRecord.MostHeadshots = playerStatistics.PersonalRecord.MostHeadshots;
								statistics.PersonalRecord.MostHealthPickedUp = playerStatistics.PersonalRecord.MostHealthPickedUp;
								statistics.PersonalRecord.MostLauncherSplats = playerStatistics.PersonalRecord.MostLauncherSplats;
								statistics.PersonalRecord.MostMachinegunSplats = playerStatistics.PersonalRecord.MostMachinegunSplats;
								statistics.PersonalRecord.MostMeleeSplats = playerStatistics.PersonalRecord.MostMeleeSplats;
								statistics.PersonalRecord.MostNutshots = playerStatistics.PersonalRecord.MostNutshots;
								statistics.PersonalRecord.MostShotgunSplats = playerStatistics.PersonalRecord.MostShotgunSplats;
								statistics.PersonalRecord.MostSniperSplats = playerStatistics.PersonalRecord.MostSniperSplats;
								statistics.PersonalRecord.MostSplats = playerStatistics.PersonalRecord.MostSplats;
								statistics.PersonalRecord.MostSplattergunSplats = playerStatistics.PersonalRecord.MostSplattergunSplats;
								statistics.PersonalRecord.MostXPEarned = playerStatistics.PersonalRecord.MostXPEarned;

								DatabaseManager.PlayerStatistics.DeleteMany(_ => _.Cmid == steamMember.Cmid);
								DatabaseManager.PlayerStatistics.Insert(statistics);
							} else {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								return outputStream.ToArray();
							}

							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);

							return outputStream.ToArray();
						} else {
							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.MemberNotFound);
							return outputStream.ToArray();
						}
					}

					throw new NotImplementedException();
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}
	}
}
