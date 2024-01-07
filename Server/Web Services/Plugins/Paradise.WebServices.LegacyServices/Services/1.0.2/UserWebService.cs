using Cmune.DataCenter.Common.Entities;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UberStrike.Core.Serialization.Legacy;
using UberStrike.Core.ViewModel;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.LegacyServices._102 {
	public class UserWebService : BaseWebService, IUserWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(UserWebService));

		public override string ServiceName => "UserWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IUserWebServiceContract);

		//private static ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		private List<PlayerLevelCapView> levelCaps;

		private FileSystemWatcher watcher;
		private static readonly List<string> watchedFiles = new List<string> {
			"LevelCaps.json",
		};

		public UserWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			try {
				levelCaps = JsonConvert.DeserializeObject<List<PlayerLevelCapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "LevelCaps.json")));

				watcher = new FileSystemWatcher(ServiceDataPath) {
					NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
				};

				watcher.Changed += (object sender, FileSystemEventArgs e) => {
					if (!watchedFiles.Contains(e.Name))
						return;

					Task.Run(async () => {
						await Task.Delay(500);
						Log.Info("Reloading Application service data due to file changes.");

						levelCaps = JsonConvert.DeserializeObject<List<PlayerLevelCapView>>(File.ReadAllText(Path.Combine(ServiceDataPath, "LevelCaps.json")));
					});
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

		#region IUserWebServiceContract
		public byte[] ChangeMemberName(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var name = StringProxy.Deserialize(bytes);
					var locale = StringProxy.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, name, locale, machineId);

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

		public byte[] GenerateNonDuplicatedMemberNames(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var username = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), username);

					using (var outputStream = new MemoryStream()) {
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

		public byte[] GetMemberWallet(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid);

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

		public byte[] GetInventory(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid);

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(cmid));

						if (userAccount != null) {
							var playerInventoryItems = DatabaseClient.PlayerInventoryItems.Find(_ => _.Cmid.Equals(userAccount.Cmid) && (_.ExpirationDate == null || _.ExpirationDate >= DateTime.UtcNow)).ToList();

							ListProxy<ItemInventoryView>.Serialize(outputStream, playerInventoryItems, ItemInventoryViewProxy.Serialize);
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

		public byte[] GetCurrencyDeposits(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, pageIndex, elementPerPage);

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

		public byte[] GetItemTransactions(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, pageIndex, elementPerPage);

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

		public byte[] GetPointsDeposits(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var pageIndex = Int32Proxy.Deserialize(bytes);
					var elementPerPage = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, pageIndex, elementPerPage);

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

		public byte[] SetScore(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid);

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

		public byte[] GetMember(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid);

					using (var outputStream = new MemoryStream()) {
						//UberstrikeUserViewModelProxy.Serialize(outputStream, new UberstrikeUserViewModel {
						//	CmuneMemberView = new MemberView {
						//		PublicProfile = new PublicProfileView {
						//			Cmid = 1,
						//			Name = "test",
						//			AccessLevel = MemberAccessLevel.Admin,
						//			EmailAddressStatus = EmailAddressStatus.Verified
						//		},
						//		MemberWallet = new MemberWalletView {
						//			Cmid = 1,
						//			Credits = 1337,
						//			CreditsExpiration = DateTime.Now,
						//			Points = 1337,
						//			PointsExpiration = DateTime.Now
						//		},
						//		MemberItems = new List<int> {
						//			1094
						//		},
						//	},
						//	UberstrikeMemberView = new UberstrikeMemberView {
						//		PlayerCardView = new PlayerCardView {
						//			Cmid = 1
						//		},
						//		PlayerStatisticsView = new PlayerStatisticsView {
						//			Cmid = 1
						//		}
						//	}
						//});

						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(cmid));

						if (userAccount != null) {
							var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid.Equals(userAccount.Cmid));
							var memberWallet = DatabaseClient.MemberWallets.FindOne(_ => _.Cmid.Equals(userAccount.Cmid));
							var memberItems = DatabaseClient.PlayerInventoryItems.Find(_ => _.Cmid.Equals(userAccount.Cmid)).Select(_ => _.ItemId).ToList();
							var playerStatistics = DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid.Equals(userAccount.Cmid));

							UberstrikeUserViewModelProxy.Serialize(outputStream, new UberstrikeUserViewModel {
								CmuneMemberView = new MemberView {
									PublicProfile = publicProfile,
									MemberWallet = memberWallet,
									MemberItems = memberItems
								},
								UberstrikeMemberView = new UberstrikeMemberView {
									PlayerStatisticsView = playerStatistics
								}
							});
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

		public byte[] GetLoadout(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid);

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(cmid));

						if (userAccount != null) {
							var playerLoadout = DatabaseClient.PlayerLoadouts.FindOne(_ => _.Cmid.Equals(userAccount.Cmid));

							if (playerLoadout == null) {
								playerLoadout = new LoadoutView {
									Cmid = userAccount.Cmid,
									MeleeWeapon = 1000, // TheSplatbat
									Weapon1 = 1002,     // MachineGun
									Weapon2 = 1003,     // ShotGun
									Weapon3 = 1004      // SniperRifle
								};

								DatabaseClient.PlayerLoadouts.Insert(playerLoadout);
							}

							var playerInventory = DatabaseClient.PlayerInventoryItems.Find(_ => _.Cmid.Equals(userAccount.Cmid));
							playerLoadout = FilterLoadout(playerLoadout, playerInventory);

							LoadoutViewProxy.Serialize(outputStream, playerLoadout);
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

		public byte[] SetLoadout(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var loadoutView = LoadoutViewProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), loadoutView);

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(loadoutView.Cmid));

						if (userAccount != null) {
							var playerInventory = DatabaseClient.PlayerInventoryItems.Find(_ => _.Cmid == userAccount.Cmid);

							loadoutView = FilterLoadout(loadoutView, playerInventory);

							var playerLoadout = DatabaseClient.PlayerLoadouts.FindOne(_ => _.Cmid == userAccount.Cmid);
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
						} else {
							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.MemberNotFound);
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

		public byte[] GetXPEventsView(byte[] data) {
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

		public byte[] GetLevelCapsView(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

					using (var outputStream = new MemoryStream()) {
						ListProxy<PlayerLevelCapView>.Serialize(outputStream, levelCaps, PlayerLevelCapViewProxy.Serialize);

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

		/// <summary>
		/// Filters illegally acquired items from a player's loadout
		/// </summary>
		private LoadoutView FilterLoadout(LoadoutView loadoutView, IEnumerable<ItemInventoryView> playerInventory) {
			if (loadoutView.UpperBody != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.UpperBody) == null) loadoutView.UpperBody = 0;
			if (loadoutView.Weapon1 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Weapon1) == null) loadoutView.Weapon1 = 0;
			if (loadoutView.Weapon2 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Weapon2) == null) loadoutView.Weapon2 = 0;
			if (loadoutView.Weapon3 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Weapon3) == null) loadoutView.Weapon3 = 0;
			if (loadoutView.QuickItem3 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.QuickItem3) == null) loadoutView.QuickItem3 = 0;
			if (loadoutView.QuickItem2 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.QuickItem2) == null) loadoutView.QuickItem2 = 0;
			if (loadoutView.QuickItem1 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.QuickItem1) == null) loadoutView.QuickItem1 = 0;
			if (loadoutView.MeleeWeapon != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.MeleeWeapon) == null) loadoutView.MeleeWeapon = 0;
			if (loadoutView.LowerBody != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.LowerBody) == null) loadoutView.LowerBody = 0;
			if (loadoutView.Head != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Head) == null) loadoutView.Head = 0;
			if (loadoutView.Gloves != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Gloves) == null) loadoutView.Gloves = 0;
			if (loadoutView.FunctionalItem3 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.FunctionalItem3) == null) loadoutView.FunctionalItem3 = 0;
			if (loadoutView.FunctionalItem2 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.FunctionalItem2) == null) loadoutView.FunctionalItem2 = 0;
			if (loadoutView.FunctionalItem1 != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.FunctionalItem1) == null) loadoutView.FunctionalItem1 = 0;
			if (loadoutView.Face != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Face) == null) loadoutView.Face = 0;
			if (loadoutView.Boots != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Boots) == null) loadoutView.Boots = 0;
			if (loadoutView.Backpack != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Backpack) == null) loadoutView.Backpack = 0;
			if (loadoutView.Webbing != 0 && playerInventory.FirstOrDefault(_ => _.ItemId == loadoutView.Webbing) == null) loadoutView.Webbing = 0;

			return loadoutView;
		}
	}
}
