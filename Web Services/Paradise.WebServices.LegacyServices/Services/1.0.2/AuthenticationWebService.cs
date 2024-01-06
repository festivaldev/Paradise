using BCrypt.Net;
using Cmune.DataCenter.Common.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using UberStrike.Core.Serialization.Legacy;
using UberStrike.Core.ViewModel;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.LegacyServices._102 {
	public class AuthenticationWebService : BaseWebService, IAuthenticationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(AuthenticationWebService));

		public override string ServiceName => "AuthenticationWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IAuthenticationWebServiceContract);

		private static ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		private WeeklySpecialView weeklySpecial = new WeeklySpecialView {
			StartDate = DateTime.MinValue,
			EndDate = DateTime.MaxValue,
			Id = 0,
			ImageUrl = "http://via.placeholder.com/350x150",
			Text = "LockWatch 2 Beta (iOS 13/14)",
			Title = "Team FESTIVAL",
			ItemId = 1003
		};

		public AuthenticationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		#region IAuthenticateWebServiceContract
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
						if (DatabaseClient.UserAccounts.FindOne(_ => _.EmailAddress == emailAddress) != null) {
							EnumProxy<MemberRegistrationResult>.Serialize(outputStream, MemberRegistrationResult.DuplicateEmail);
						} else {
							var r = new Random((int)DateTime.UtcNow.Ticks);
							var Cmid = r.Next(1, int.MaxValue);

							var userAccount = new UserAccount {
								Cmid = Cmid,
								EmailAddress = emailAddress,
								Password = BCrypt.Net.BCrypt.HashPassword(password),
								Channel = channel,
								Locale = locale
							};

							DatabaseClient.UserAccounts.Insert(userAccount);

							EnumProxy<MemberRegistrationResult>.Serialize(outputStream, MemberRegistrationResult.Ok);
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
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(cmid));

						if (userAccount != null) {
							var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid.Equals(userAccount.Cmid));

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

								var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == publicProfile.Cmid);
								memberWallet.Points += 2000;

								DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == publicProfile.Cmid);
								DatabaseClient.MemberWallets.Insert(memberWallet);

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

		public byte[] LoginMemberEmail(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var email = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var channelType = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), email, password, channelType, machineId);

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.EmailAddress == email);

						if (userAccount == null) {
							MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
								MemberAuthenticationResult = MemberAuthenticationResult.InvalidEmail
							});
						} else {
							var bannedMember = DatabaseClient.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == userAccount.Cmid);

							if (bannedMember != null && (bannedMember.ExpireTime == null || bannedMember.ExpireTime > DateTime.UtcNow)) {
								MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
									MemberAuthenticationResult = MemberAuthenticationResult.IsBanned
								});
							} else {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == userAccount.Cmid);
								var newPlayer = publicProfile == null;

								if (newPlayer) {
									publicProfile = new PublicProfileView {
										Cmid = userAccount.Cmid,
										Name = string.Empty,
										LastLoginDate = DateTime.UtcNow,
										EmailAddressStatus = EmailAddressStatus.Verified
									};

									DatabaseClient.PublicProfiles.Insert(publicProfile);

									var memberWallet = new MemberWalletView {
										Cmid = userAccount.Cmid,
										//Points = 10000,
										//Credits = 1000,
										PointsExpiration = DateTime.MaxValue,
										CreditsExpiration = DateTime.MaxValue
									};

									DatabaseClient.MemberWallets.Insert(memberWallet);

									//var transactionKey = new byte[32];
									//new Random((int)DateTime.UtcNow.Ticks).NextBytes(transactionKey);

									//var builder = new StringBuilder(64);
									//for (int i = 0; i < transactionKey.Length; i++) {
									//	builder.Append(transactionKey[i].ToString("x2"));
									//}

									//DatabaseClient.CurrencyDeposits.Insert(new CurrencyDepositView {
									//	BundleName = "Signup Reward",
									//	Cmid = userAccount.Cmid,
									//	Credits = memberWallet.Credits,
									//	CurrencyLabel = "$",
									//	DepositDate = DateTime.UtcNow,
									//	Points = memberWallet.Points,
									//	TransactionKey = builder.ToString(),
									//});

									var playerStatistics = new PlayerStatisticsView {
										Cmid = userAccount.Cmid,
										PersonalRecord = new PlayerPersonalRecordStatisticsView(),
										WeaponStatistics = new PlayerWeaponStatisticsView()
									};

									DatabaseClient.PlayerStatistics.Insert(playerStatistics);

									var session = GameSessionManager.Instance.FindOrCreateSession(publicProfile, machineId, userAccount);

									var memberAuth = new MemberAuthenticationResultView {
										MemberAuthenticationResult = MemberAuthenticationResult.Ok,
										MemberView = new MemberView {
											PublicProfile = publicProfile,
											MemberWallet = memberWallet
										},
										PlayerStatisticsView = playerStatistics,
										ServerTime = DateTime.UtcNow,
										IsAccountComplete = false,
										IsTutorialComplete = false,
										AuthToken = session.SessionId,
										WeeklySpecial = weeklySpecial
									};

									MemberAuthenticationResultViewProxy.Serialize(outputStream, memberAuth);
								} else {
									var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == userAccount.Cmid);
									var playerStatistics = DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == userAccount.Cmid);

									var session = GameSessionManager.Instance.FindOrCreateSession(publicProfile, machineId, userAccount);

									MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
										MemberAuthenticationResult = MemberAuthenticationResult.Ok,
										MemberView = new MemberView {
											PublicProfile = publicProfile,
											MemberWallet = memberWallet
										},
										PlayerStatisticsView = playerStatistics,
										ServerTime = DateTime.UtcNow,
										IsAccountComplete = !string.IsNullOrWhiteSpace(publicProfile.Name),
										IsTutorialComplete = userAccount.TutorialStep >= UberStrike.Core.Types.TutorialStepType.TutorialComplete,
										AuthToken = session.SessionId,
										WeeklySpecial = weeklySpecial
									});

									if (!string.IsNullOrWhiteSpace(publicProfile.Name)) {
										if (DatabaseClient.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid == userAccount.Cmid) != null) is ClanView clan) {
											var clanMember = clan.Members.Find(_ => _.Cmid == userAccount.Cmid);

											clanMember.Lastlogin = DateTime.UtcNow;

											DatabaseClient.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
											DatabaseClient.Clans.Insert(clan);
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

		public byte[] LoginMemberCookie(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var expirationTime = DateTimeProxy.Deserialize(bytes);
					var encryptedContent = StringProxy.Deserialize(bytes);
					var hash = StringProxy.Deserialize(bytes);
					var channelType = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, expirationTime, encryptedContent, hash, channelType, machineId);

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

		public byte[] LoginMemberFacebook(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var facebookId = StringProxy.Deserialize(bytes);
					var hash = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), facebookId, hash, machineId);

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

		public byte[] FacebookSingleSignOn(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var channelType = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, channelType, machineId);

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
