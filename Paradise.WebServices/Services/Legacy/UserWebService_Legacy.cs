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
	public class UserWebService_Legacy : WebServiceBase, IUserWebServiceContract_Legacy {
		public override string ServiceName => "UserWebService";
		public override string ServiceVersion => "1.0.1";
		protected override Type ServiceInterface => typeof(IUserWebServiceContract_Legacy);

		public UserWebService_Legacy(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() { }

		public byte[] ChangeMemberName(byte[] data) {
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

		public byte[] FindMembers(byte[] data) {
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

		public byte[] GenerateNonDuplicatedMemberNames(byte[] data) {
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

		public byte[] GetCurrencyDeposits(byte[] data) {
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

		public byte[] GetInventory(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						ListProxy<ItemInventoryView>.Serialize(outputStream, new List<ItemInventoryView> {
							new ItemInventoryView {
								ItemId = 1094,
							}
						}, ItemInventoryViewProxy.Serialize);

						//return outputStream.ToArray();
						return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=", "aaaabbbbccccdddd");
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] GetItemTransactions(byte[] data) {
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

		public byte[] GetLevelCapsView(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						ListProxy<PlayerLevelCapView>.Serialize(outputStream, new List<PlayerLevelCapView> {
							new PlayerLevelCapView {
								Level = 1,
								XPRequired = 1000
							}
						}, PlayerLevelCapViewProxy.Serialize);

						//return outputStream.ToArray();
						return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=", "aaaabbbbccccdddd");
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] GetLoadout(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						LoadoutViewProxy.Serialize(outputStream, new LoadoutView { });

						//return outputStream.ToArray();
						return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=", "aaaabbbbccccdddd");
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] GetMember(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

					using (var outputStream = new MemoryStream()) {
						//MemberViewProxy.Serialize(outputStream, new MemberView {
						//	PublicProfile = new PublicProfileView {
						//		Cmid = 1,
						//		Name = "test",
						//		AccessLevel = MemberAccessLevel.Admin,
						//		EmailAddressStatus = EmailAddressStatus.Verified
						//	},
						//	MemberWallet = new MemberWalletView {
						//		Cmid = 1,
						//		Credits = 1337,
						//		CreditsExpiration = DateTime.Now,
						//		Points = 1337,
						//		PointsExpiration = DateTime.Now
						//	},
						//	MemberItems = new List<int> {
						//		1094
						//	},
						//});

						UberstrikeUserViewModelProxy.Serialize(outputStream, new UberstrikeUserViewModel {
							CmuneMemberView = new MemberView {
								PublicProfile = new PublicProfileView {
									Cmid = 1,
									Name = "test",
									AccessLevel = MemberAccessLevel.Admin,
									EmailAddressStatus = EmailAddressStatus.Verified
								},
								MemberWallet = new MemberWalletView {
									Cmid = 1,
									Credits = 1337,
									CreditsExpiration = DateTime.Now,
									Points = 1337,
									PointsExpiration = DateTime.Now
								},
								MemberItems = new List<int> {
									1094
								},
							},
							UberstrikeMemberView = new UberstrikeMemberView {
								PlayerCardView = new PlayerCardView {
									Cmid = 1
								},
								PlayerStatisticsView = new PlayerStatisticsView {
									Cmid = 1
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

		public byte[] GetMemberWallet(byte[] data) {
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

		public byte[] GetPointsDeposits(byte[] data) {
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

		public byte[] GetPublicProfile(byte[] data) {
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

		public byte[] GetRealTimeStatistics(byte[] data) {
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

		public byte[] GetStatistics(byte[] data) {
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

		public byte[] GetUserAndTopStats(byte[] data) {
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

		public byte[] GetXPEventsView(byte[] data) {
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

		public byte[] IsDuplicateMemberName(byte[] data) {
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

		public byte[] ReportMember(byte[] data) {
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

		public byte[] SetLoadout(byte[] data) {
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

		public byte[] SetScore(byte[] data) {
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