using Cmune.DataCenter.Common.Entities;
using log4net;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using UberStrike.Core.Serialization;
using UberStrike.Core.ViewModel;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.Services {
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
	public class AuthenticationWebService : BaseWebService, IAuthenticationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(AuthenticationWebService));

		public override string ServiceName => "AuthenticationWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IAuthenticationWebServiceContract);

		private static readonly ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		public AuthenticationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		#region IAuthenticateWebServiceContract
		/// <summary>
		/// Completes a just created account by setting the player's name and rewarding them the default starting items
		/// </summary>
		public byte[] CompleteAccount(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var name = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var locale = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, name, channel, locale, machineId);

					using (var outputStream = new MemoryStream()) {
						var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == cmid);

						if (publicProfile.Name != null) {
							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.AlreadyCompletedAccount
							});
						} else if (DatabaseClient.PublicProfiles.FindOne(_ => _.Name == name) != null) {
							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.DuplicateName
							});
						} else if (name.Length < 3 || !Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$")) {
							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.InvalidName
							});
						} else if (ProfanityFilter.DetectAllProfanities(name).Count > 0) {
							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.InvalidName
							});
						} else {
							publicProfile.Name = name;
							DatabaseClient.PublicProfiles.Update(publicProfile);

							DatabaseClient.PlayerInventoryItems.InsertBulk(new List<ItemInventoryView> {
								new ItemInventoryView {
									Cmid = cmid,
									ItemId = (int)UberstrikeInventoryItem.TheSplatbat,
									AmountRemaining = -1
								},
								new ItemInventoryView {
									Cmid = cmid,
									ItemId = (int)UberstrikeInventoryItem.MachineGun,
									AmountRemaining = -1
								},
								new ItemInventoryView {
									Cmid = cmid,
									ItemId = (int)UberstrikeInventoryItem.ShotGun,
									AmountRemaining = -1
								},
								new ItemInventoryView {
									Cmid = cmid,
									ItemId = (int)UberstrikeInventoryItem.SniperRifle,
									AmountRemaining = -1
								}
							});

							DatabaseClient.ItemTransactions.InsertBulk(new List<ItemTransactionView> {
								new ItemTransactionView {
									Cmid = publicProfile.Cmid,
									Credits = 0,
									Duration = BuyingDurationType.Permanent,
									ItemId = (int)UberstrikeInventoryItem.TheSplatbat,
									Points = 0,
									WithdrawalDate = DateTime.UtcNow
								},
								new ItemTransactionView {
									Cmid = publicProfile.Cmid,
									Credits = 0,
									Duration = BuyingDurationType.Permanent,
									ItemId = (int)UberstrikeInventoryItem.MachineGun,
									Points = 0,
									WithdrawalDate = DateTime.UtcNow
								},
								new ItemTransactionView {
									Cmid = publicProfile.Cmid,
									Credits = 0,
									Duration = BuyingDurationType.Permanent,
									ItemId = (int)UberstrikeInventoryItem.ShotGun,
									Points = 0,
									WithdrawalDate = DateTime.UtcNow
								},
								new ItemTransactionView {
									Cmid = publicProfile.Cmid,
									Credits = 0,
									Duration = BuyingDurationType.Permanent,
									ItemId = (int)UberstrikeInventoryItem.SniperRifle,
									Points = 0,
									WithdrawalDate = DateTime.UtcNow
								}
							});

							var playerLoadout = DatabaseClient.PlayerLoadouts.FindOne(_ => _.Cmid == cmid);
							playerLoadout.MeleeWeapon = (int)UberstrikeInventoryItem.TheSplatbat;
							playerLoadout.Weapon1 = (int)UberstrikeInventoryItem.MachineGun;

							DatabaseClient.PlayerLoadouts.Update(playerLoadout);

							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.Ok,
								ItemsAttributed = new Dictionary<int, int> {
									[(int)UberstrikeInventoryItem.TheSplatbat] = 1,
									[(int)UberstrikeInventoryItem.MachineGun] = 1,
									[(int)UberstrikeInventoryItem.ShotGun] = 1,
									[(int)UberstrikeInventoryItem.SniperRifle] = 1,
								}
							});

							Log.Info($"{publicProfile.Name}({publicProfile.Cmid}) logged in.");
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
		/// Creates a new user object when a player chooses to sign in using a UberStrike account
		/// </summary>
		/// <remarks>
		///	Since the official UberStrike servers are shut down, this will probably be replaced with signing in using a Team FESTIVAL account
		/// </remarks>
		public byte[] CreateUser(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var emailAddress = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var locale = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), emailAddress, password, channel, locale, machineId);

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
		/// Links a Steam member to an existing UberStrike account
		/// </summary>
		public byte[] LinkSteamMember(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var email = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var steamId = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), email, password, steamId, machineId);

					using (var outputStream = new MemoryStream()) {
						/// IDEA: Login using account.festival.tf?
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
		/// Provides the mechanism to sign in via E-Mail and password
		/// </summary>
		public byte[] LoginMemberEmail(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var email = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), email, password, channel, machineId);

					using (var outputStream = new MemoryStream()) {
						/// IDEA: Login using account.festival.tf?
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
		/// Provides the mechanism to sign in via the Facebook SDK (screw you Meta, btw)
		/// </summary>
		/// <remarks>
		///	Since UberStrike is not available on Facebook anymore, this will not be implemented
		/// </remarks>
		public byte[] LoginMemberFacebookUnitySdk(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var facebookPlayerAccessToken = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), facebookPlayerAccessToken, channel, machineId);

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
		/// Probably provides the mechanism to sign in via a UberStrike account from their website
		/// </summary>
		/// <remarks>
		///	Since the UberStrike website has been shut down, this will not be implemented
		/// </remarks>
		public byte[] LoginMemberPortal(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var hash = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, hash, machineId);

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
		/// Authenticates users via Steam
		/// </summary>
		public byte[] LoginSteam(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var steamId = StringProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), steamId, authToken, machineId);

					using (var outputStream = new MemoryStream()) {
						var steamMember = DatabaseClient.SteamMembers.FindOne(_ => _.SteamId == steamId);

						if (steamMember == null) {
							var r = new Random((int)DateTime.UtcNow.Ticks);
							var Cmid = r.Next(1, int.MaxValue);

							steamMember = new SteamMember {
								SteamId = steamId,
								Cmid = Cmid,
								MachineId = machineId
							};

							DatabaseClient.SteamMembers?.Insert(steamMember);

							var publicProfile = new PublicProfileView {
								Cmid = Cmid,
								Name = string.Empty,
								LastLoginDate = DateTime.UtcNow,
								EmailAddressStatus = EmailAddressStatus.Verified
							};

							DatabaseClient.PublicProfiles.Insert(publicProfile);

							var memberWallet = new MemberWalletView {
								Cmid = Cmid,
								Points = 10000,
								Credits = 1000,
								PointsExpiration = DateTime.MaxValue,
								CreditsExpiration = DateTime.MaxValue
							};

							DatabaseClient.MemberWallets.Insert(memberWallet);

							var transactionKey = new byte[32];
							new Random((int)DateTime.UtcNow.Ticks).NextBytes(transactionKey);

							var builder = new StringBuilder(64);
							for (int i = 0; i < transactionKey.Length; i++) {
								builder.Append(transactionKey[i].ToString("x2"));
							}

							DatabaseClient.CurrencyDeposits.Insert(new CurrencyDepositView {
								BundleName = "Signup Reward",
								Cmid = Cmid,
								Credits = memberWallet.Credits,
								CurrencyLabel = "$",
								DepositDate = DateTime.UtcNow,
								Points = memberWallet.Points,
								TransactionKey = builder.ToString(),
							});

							var playerStatistics = new PlayerStatisticsView {
								Cmid = Cmid,
								PersonalRecord = new PlayerPersonalRecordStatisticsView(),
								WeaponStatistics = new PlayerWeaponStatisticsView()
							};

							DatabaseClient.PlayerStatistics.Insert(playerStatistics);

							var session = GameSessionManager.Instance.FindOrCreateSession(publicProfile, machineId, steamMember);

							var memberAuth = new MemberAuthenticationResultView {
								MemberAuthenticationResult = MemberAuthenticationResult.Ok,
								MemberView = new MemberView {
									PublicProfile = publicProfile,
									MemberWallet = memberWallet
								},
								PlayerStatisticsView = playerStatistics,
								ServerTime = DateTime.UtcNow,
								IsAccountComplete = false,
								AuthToken = session.SessionId
							};

							MemberAuthenticationResultViewProxy.Serialize(outputStream, memberAuth);
						} else {
							var bannedMember = DatabaseClient.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == steamMember.Cmid);

							if (bannedMember != null && (bannedMember.ExpireTime == null || bannedMember.ExpireTime > DateTime.UtcNow)) {
								MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
									MemberAuthenticationResult = MemberAuthenticationResult.IsBanned
								});
							} else {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (publicProfile == null) {
									MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
										MemberAuthenticationResult = MemberAuthenticationResult.UnknownError
									});
								} else {
									var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);
									var playerStatistics = DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

									var session = GameSessionManager.Instance.FindOrCreateSession(publicProfile, machineId, steamMember);

									MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
										MemberAuthenticationResult = MemberAuthenticationResult.Ok,
										MemberView = new MemberView {
											PublicProfile = publicProfile,
											MemberWallet = memberWallet
										},
										PlayerStatisticsView = playerStatistics,
										ServerTime = DateTime.UtcNow,
										IsAccountComplete = !string.IsNullOrWhiteSpace(publicProfile.Name),
										AuthToken = session.SessionId
									});

									if (!string.IsNullOrWhiteSpace(publicProfile.Name)) {
										if (DatabaseClient.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid == steamMember.Cmid) != null) is ClanView clan) {
											var clanMember = clan.Members.Find(_ => _.Cmid == steamMember.Cmid);

											clanMember.Lastlogin = DateTime.UtcNow;

											//DatabaseClient.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
											//DatabaseClient.Clans.Insert(clan);
											DatabaseClient.Clans.Update(clan);
										}

										Log.Info($"{publicProfile.Name}({publicProfile.Cmid}) logged in.");
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

		public byte[] VerifyAuthToken(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (!GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
								MemberAuthenticationResult = MemberAuthenticationResult.InvalidCookie
							});
						} else {
							var steamMember = session.SteamMember;

							if (steamMember == null) {
								MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
									MemberAuthenticationResult = MemberAuthenticationResult.UnknownError
								});
							} else {
								var bannedMember = DatabaseClient.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == steamMember.Cmid);

								if (bannedMember != null && (bannedMember.ExpireTime == null || bannedMember.ExpireTime > DateTime.UtcNow)) {
									MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
										MemberAuthenticationResult = MemberAuthenticationResult.IsBanned
									});
								} else {
									var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

									if (publicProfile == null) {
										MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
											MemberAuthenticationResult = MemberAuthenticationResult.UnknownError
										});
									} else {
										var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);
										var playerStatistics = DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

										MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
											MemberAuthenticationResult = MemberAuthenticationResult.Ok,
											MemberView = new MemberView {
												PublicProfile = publicProfile,
												MemberWallet = memberWallet
											},
											PlayerStatisticsView = playerStatistics,
											ServerTime = DateTime.UtcNow,
											IsAccountComplete = publicProfile.Name != null,
											AuthToken = session.SessionId
										});

										if (DatabaseClient.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid == steamMember.Cmid) != null) is ClanView clan) {
											var clanMember = clan.Members.Find(_ => _.Cmid == steamMember.Cmid);

											clanMember.Lastlogin = DateTime.UtcNow;

											//DatabaseClient.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
											//DatabaseClient.Clans.Insert(clan);
											DatabaseClient.Clans.Update(clan);
										}
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
	}
	#endregion
}