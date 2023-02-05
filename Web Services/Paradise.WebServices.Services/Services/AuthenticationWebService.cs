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
	public class AuthenticationWebService : BaseWebService, IAuthenticationWebServiceContract {
		public override string ServiceName => "AuthenticationWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IAuthenticationWebServiceContract);

		private static ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		public AuthenticationWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public AuthenticationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		/// <summary>
		/// Completes a just created account by setting the player's name and rewarding them the default starting items
		/// </summary>
		public byte[] CompleteAccount(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var name = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var locale = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(cmid, name, channel, locale, machineId);

					using (var outputStream = new MemoryStream()) {
						var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid);

						if (publicProfile.Name != null) {
							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.AlreadyCompletedAccount
							});
						} else if (DatabaseManager.PublicProfiles.FindOne(_ => _.Name == name) != null) {
							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.DuplicateName
							});
						} else if (ProfanityFilter.ContainsProfanity(name)) {
							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.InvalidName
							});
						} else {
							publicProfile.Name = name;
							DatabaseManager.PublicProfiles.Update(publicProfile);

							DatabaseManager.PlayerInventoryItems.InsertBulk(new List<ItemInventoryView> {
								new ItemInventoryView {
									Cmid = cmid,
									ItemId = (int)UberstrikeInventoryItem.TheSplatbat,
									AmountRemaining = -1
								},
								new ItemInventoryView {
									Cmid = cmid,
									ItemId = (int)UberstrikeInventoryItem.MachineGun,
									AmountRemaining = -1
								}
							});

							DatabaseManager.ItemTransactions.InsertBulk(new List<ItemTransactionView> {
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
								}
							});

							var playerLoadout = DatabaseManager.PlayerLoadouts.FindOne(_ => _.Cmid == cmid);
							playerLoadout.MeleeWeapon = (int)UberstrikeInventoryItem.TheSplatbat;
							playerLoadout.Weapon1 = (int)UberstrikeInventoryItem.MachineGun;

							DatabaseManager.PlayerLoadouts.Update(playerLoadout);

							AccountCompletionResultViewProxy.Serialize(outputStream, new AccountCompletionResultView {
								Result = AccountCompletionResult.Ok,
								ItemsAttributed = new Dictionary<int, int> {
									[(int)UberstrikeInventoryItem.TheSplatbat] = 1,
									[(int)UberstrikeInventoryItem.MachineGun] = 1
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
		/// Creates a new user object when a player chooses to sign in using a UberStrike account
		/// </summary>
		/// <remarks>
		///	Since the official UberStrike servers are shut down, this will probably be replaced with signing in using a Team FESTIVAL account
		/// </remarks>
		public byte[] CreateUser(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var emailAddress = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var locale = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(emailAddress, password, channel, locale, machineId);

					using (var outputStream = new MemoryStream()) {
						throw new NotImplementedException();

						//return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var email = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var steamId = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(email, password, steamId, machineId);

					using (var outputStream = new MemoryStream()) {
						/// IDEA: Login using account.festival.tf?
						throw new NotImplementedException();

						//return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var email = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(email, password, channel, machineId);

					using (var outputStream = new MemoryStream()) {
						/// IDEA: Login using account.festival.tf?
						throw new NotImplementedException();

						//return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var facebookPlayerAccessToken = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(facebookPlayerAccessToken, channel, machineId);

					using (var outputStream = new MemoryStream()) {
						throw new NotImplementedException();

						//return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var hash = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(cmid, hash, machineId);

					using (var outputStream = new MemoryStream()) {
						throw new NotImplementedException();

						//return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var steamId = StringProxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(steamId, authToken, machineId);

					//Console.WriteLine($"[{ServiceName}] Authenticating Steam user (SteamID: {steamId}, auth token: {authToken}, machine ID: {machineId}");

					using (var outputStream = new MemoryStream()) {
						string authTicket;
						using (var authTicketStream = new MemoryStream()) {
							StringProxy.Serialize(authTicketStream, steamId);
							DateTimeProxy.Serialize(authTicketStream, DateTime.UtcNow.AddDays(2));

							authTicket = Convert.ToBase64String(authTicketStream.ToArray());
						}

						var steamMember = DatabaseManager.SteamMembers.FindOne(_ => _.SteamId == steamId);

						if (steamMember == null) {
							var r = new Random((int)DateTime.UtcNow.Ticks);
							var Cmid = r.Next(1, int.MaxValue);

							steamMember = new SteamMember {
								SteamId = steamId,
								Cmid = Cmid,
								MachineId = machineId
							};

							DatabaseManager.SteamMembers?.Insert(steamMember);

							var publicProfile = new PublicProfileView {
								Cmid = Cmid,
								Name = string.Empty,
								LastLoginDate = DateTime.UtcNow,
								EmailAddressStatus = EmailAddressStatus.Verified,
								GroupTag = string.Empty,
								FacebookId = string.Empty
							};

							DatabaseManager.PublicProfiles.Insert(publicProfile);

							var memberWallet = new MemberWalletView {
								Cmid = Cmid,
								Points = 10000,
								Credits = 1000,
								PointsExpiration = DateTime.MaxValue,
								CreditsExpiration = DateTime.MaxValue
							};

							DatabaseManager.MemberWallets.Insert(memberWallet);

							var transactionKey = new byte[32];
							new Random((int)DateTime.UtcNow.Ticks).NextBytes(transactionKey);

							var builder = new StringBuilder(64);
							for (int i = 0; i < transactionKey.Length; i++) {
								builder.Append(transactionKey[i].ToString("x2"));
							}

							DatabaseManager.CurrencyDeposits.Insert(new CurrencyDepositView {
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

							DatabaseManager.PlayerStatistics.Insert(playerStatistics);

							var memberAuth = new MemberAuthenticationResultView {
								MemberAuthenticationResult = MemberAuthenticationResult.Ok,
								MemberView = new MemberView {
									PublicProfile = publicProfile,
									MemberWallet = memberWallet
								},
								PlayerStatisticsView = playerStatistics,
								ServerTime = DateTime.UtcNow,
								IsAccountComplete = false,
								AuthToken = authTicket
							};

							MemberAuthenticationResultViewProxy.Serialize(outputStream, memberAuth);
						} else {
							var bannedMember = DatabaseManager.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == steamMember.Cmid);

							if (bannedMember != null && (bannedMember.ExpireTime == null || bannedMember.ExpireTime > DateTime.UtcNow)) {
								MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
									MemberAuthenticationResult = MemberAuthenticationResult.IsBanned
								});
							} else {
								var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (publicProfile == null) {
									MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
										MemberAuthenticationResult = MemberAuthenticationResult.UnknownError
									});
								} else {
									var memberWallet = DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == steamMember.Cmid);
									var playerStatistics = DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);

									MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView {
										MemberAuthenticationResult = MemberAuthenticationResult.Ok,
										MemberView = new MemberView {
											PublicProfile = publicProfile,
											MemberWallet = memberWallet
										},
										PlayerStatisticsView = playerStatistics,
										ServerTime = DateTime.UtcNow,
										IsAccountComplete = publicProfile.Name != null,
										AuthToken = authTicket
									});

									if (DatabaseManager.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid == steamMember.Cmid) != null) is ClanView clan) {
										var clanMember = clan.Members.Find(_ => _.Cmid == steamMember.Cmid);

										clanMember.Lastlogin = DateTime.UtcNow;

										DatabaseManager.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
										DatabaseManager.Clans.Insert(clan);
									}
								}
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
	}
}
