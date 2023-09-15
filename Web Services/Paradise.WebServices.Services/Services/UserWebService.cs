using log4net;
using Paradise.Core.Serialization;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class UserWebService : BaseWebService, IUserWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(UserWebService));

		public override string ServiceName => "UserWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IUserWebServiceContract);

		private static readonly ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		public UserWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		#region IUserWebServiceContract
		/// <summary>
		/// Adds an item transaction to a player's history
		/// </summary>
		public byte[] AddItemTransaction(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var itemTransaction = ItemTransactionViewProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), itemTransaction, authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								DatabaseClient.ItemTransactions.Insert(itemTransaction);

								BooleanProxy.Serialize(outputStream, true);
								return isEncrypted
									? CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector)
									: outputStream.ToArray();
							}
						}

						BooleanProxy.Serialize(outputStream, false);
						return isEncrypted
							? CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector)
							: outputStream.ToArray(); ;
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
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var name = StringProxy.Deserialize(bytes);
					var locale = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, name, locale, machineId);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
								var existingName = DatabaseClient.PublicProfiles.FindOne(_ => _.Name == name);
								var nameChangeItem = DatabaseClient.PlayerInventoryItems.FindOne(_ => _.Cmid == publicProfile.Cmid && _.ItemId == (int)UberstrikeInventoryItem.NameChange);

								if (nameChangeItem == null) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.NameChangeNotInInventory);
								} else if (existingName != null && existingName.Cmid != publicProfile.Cmid) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.DuplicateName);
								} else if (ProfanityFilter.DetectAllProfanities(name).Count > 0) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.OffensiveName);
								} else {
									publicProfile.Name = name;
									DatabaseClient.PublicProfiles.Update(publicProfile);

									DatabaseClient.PlayerInventoryItems.DeleteMany(_ => _.Cmid == publicProfile.Cmid && _.ItemId == (int)UberstrikeInventoryItem.NameChange);

									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
								}
							} else {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.MemberNotFound);
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
		/// Adds a credit deposit transaction to a player's history
		/// </summary>
		public byte[] DepositCredits(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var depositTransaction = CurrencyDepositViewProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), depositTransaction, authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (memberWallet != null) {
									if (depositTransaction.Credits > 0) {
										DatabaseClient.CurrencyDeposits.Insert(depositTransaction);

										memberWallet.Credits += depositTransaction.Credits;

										DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
										DatabaseClient.MemberWallets.Insert(memberWallet);

										BooleanProxy.Serialize(outputStream, true);
									} else {
										BooleanProxy.Serialize(outputStream, false);
									}
								}
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
		/// Adds a point deposit transaction to a player's history
		/// </summary>
		public byte[] DepositPoints(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var depositTransaction = PointDepositViewProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), depositTransaction, authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (memberWallet != null) {
									if (depositTransaction.Points > 0) {
										DatabaseClient.PointDeposits.Insert(depositTransaction);

										memberWallet.Points += depositTransaction.Points;

										DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == steamMember.Cmid);
										DatabaseClient.MemberWallets.Insert(memberWallet);

										BooleanProxy.Serialize(outputStream, true);
									} else {
										BooleanProxy.Serialize(outputStream, false);
									}
								}
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
		/// Generates a list of unique usernames in case the desired username is already in use
		/// </summary>
		public byte[] GenerateNonDuplicatedMemberNames(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var username = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), username);

					List<string> generatedUsernames = new List<string>();

					var r = new Random((int)DateTime.UtcNow.Ticks);
					while (generatedUsernames.Count < 3) {
						var number = r.Next(0, 99999);

						// Names are limited to 18 characters,
						// so limit the generated name too
						string generatedUsername = $"{username.Substring(0, Math.Min(username.Length, 18 - number.ToString().Length))}{number}";

						if (DatabaseClient.PublicProfiles.FindOne(_ => _.Name == generatedUsername) == null) {
							generatedUsernames.Add(generatedUsername);
						}
					}

					using (var outputStream = new MemoryStream()) {
						ListProxy<string>.Serialize(outputStream, generatedUsernames, StringProxy.Serialize);

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
		/// Gets a list of historical currency deposits (such as purchasing credit bundles) for the current user
		/// </summary>
		/// <seealso cref="ShopWebService.BuyBundleSteam(byte[])"/>
		public byte[] GetCurrencyDeposits(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, pageIndex, elementPerPage);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var currencyDeposits = DatabaseClient.CurrencyDeposits.Find(_ => _.Cmid == steamMember.Cmid);

								CurrencyDepositsViewModelProxy.Serialize(outputStream, new CurrencyDepositsViewModel {
									CurrencyDeposits = currencyDeposits.Skip((pageIndex - 1) * elementPerPage).Take(elementPerPage).ToList(),
									TotalCount = currencyDeposits.Count()
								});
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
		/// Returns a list of owned items
		/// </summary>
		public byte[] GetInventory(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var playerInventoryItems = DatabaseClient.PlayerInventoryItems.Find(_ => _.Cmid == steamMember.Cmid && (_.ExpirationDate == null || _.ExpirationDate >= DateTime.UtcNow)).ToList();

								ListProxy<ItemInventoryView>.Serialize(outputStream, playerInventoryItems, ItemInventoryViewProxy.Serialize);
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
		/// Gets a list of historical item transactions for the current user
		/// </summary>
		public byte[] GetItemTransactions(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, pageIndex, elementPerPage);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var itemTransactions = DatabaseClient.ItemTransactions.Find(_ => _.Cmid == steamMember.Cmid);

								ItemTransactionsViewModelProxy.Serialize(outputStream, new ItemTransactionsViewModel {
									ItemTransactions = itemTransactions.Skip((pageIndex - 1) * elementPerPage).Take(elementPerPage).ToList(),
									TotalCount = itemTransactions.Count()
								});
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
		/// Gets a players current loadout (or creates a new default loadout if it doesn't exist)
		/// </summary>
		public byte[] GetLoadout(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var playerLoadout = DatabaseClient.PlayerLoadouts.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (playerLoadout == null) {
									playerLoadout = new LoadoutView {
										Cmid = steamMember.Cmid,
										MeleeWeapon = (int)UberstrikeInventoryItem.TheSplatbat,
										Weapon1 = (int)UberstrikeInventoryItem.MachineGun
									};

									DatabaseClient.PlayerLoadouts.Insert(playerLoadout);
								}

								LoadoutViewProxy.Serialize(outputStream, playerLoadout);
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
		/// Gets public information of a player and their statistics
		/// </summary>
		public byte[] GetMember(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
								var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);
								var playerStatistics = DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

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
		/// Gets a list of (historical) sessions of a player. Appears to be unused by the game
		/// </summary>
		public byte[] GetMemberListSessionData(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authTokens = ListProxy<string>.Deserialize(bytes, new ListProxy<string>.Deserializer<string>(StringProxy.Deserialize));
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authTokens);

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
		/// Gets the current session data of a player. Appears to be unused by the game
		/// </summary>
		public byte[] GetMemberSessionData(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

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
		/// Gets the current wallet (amount of points and credits) of a player
		/// </summary>
		public byte[] GetMemberWallet(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (memberWallet != null) {
									MemberWalletViewProxy.Serialize(outputStream, memberWallet);
								}
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
		/// Gets a list of historical points deposits for the current user
		/// </summary>
		public byte[] GetPointsDeposits(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, pageIndex, elementPerPage);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var pointDeposits = DatabaseClient.PointDeposits.Find(_ => _.Cmid == steamMember.Cmid);

								PointDepositsViewModelProxy.Serialize(outputStream, new PointDepositsViewModel {
									PointDeposits = pointDeposits.Skip((pageIndex - 1) * elementPerPage).Take(elementPerPage).ToList(),
									TotalCount = pointDeposits.Count()
								});
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
		/// Checks if a username is already in use
		/// </summary>
		public byte[] IsDuplicateMemberName(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var username = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), username);

					using (var outputStream = new MemoryStream()) {
						BooleanProxy.Serialize(outputStream, DatabaseClient.PublicProfiles.FindOne(_ => _.Name == username) != null);

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
		/// Sets the loadout of the current player
		/// </summary>
		public byte[] SetLoadout(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var loadoutView = LoadoutViewProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, loadoutView);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var playerLoadout = DatabaseClient.PlayerLoadouts.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (playerLoadout != null) {
									playerLoadout.UpperBody = loadoutView.UpperBody;
									playerLoadout.Weapon1 = loadoutView.Weapon1;
									playerLoadout.Weapon2 = loadoutView.Weapon2;
									playerLoadout.Weapon3 = loadoutView.Weapon3;
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

									DatabaseClient.PlayerLoadouts.Update(playerLoadout);
								} else {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
									return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
								}

								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);

								return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
							} else {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.MemberNotFound);
								return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
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

		public byte[] UpdatePlayerStatistics(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var playerStatistics = PlayerStatisticsViewProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, playerStatistics);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var statistics = DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

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

									DatabaseClient.PlayerStatistics.DeleteMany(_ => _.Cmid == steamMember.Cmid);
									DatabaseClient.PlayerStatistics.Insert(statistics);
								} else {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
									return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
								}

								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);

								return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
							} else {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.MemberNotFound);
								return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), EncryptionPassPhrase, EncryptionInitVector);
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
		#endregion
	}
}
