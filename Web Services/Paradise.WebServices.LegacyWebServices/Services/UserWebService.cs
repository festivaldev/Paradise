using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using Paradise.Core.Serialization.Legacy;
using Paradise.Core.Types;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.WebServices.LegacyWebServices {
	public class UserWebService : BaseWebService, IUserWebServiceContract {
		public override string ServiceName => "UserWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IUserWebServiceContract);


		public UserWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			
		}

		protected override void Teardown() {

		}



		public byte[] ChangeMemberName(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] IsDuplicateMemberName(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] GenerateNonDuplicatedMemberNames(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] GetMemberWallet(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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

		public byte[] GetInventory(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(cmid);

					using (var outputStream = new MemoryStream()) {
						ListProxy<ItemInventoryView>.Serialize(outputStream, new List<ItemInventoryView>(), ItemInventoryViewProxy.Serialize);

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

						//return outputStream.ToArray();
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

						//return outputStream.ToArray();
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

						//return outputStream.ToArray();
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
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(cmid);

					using (var outputStream = new MemoryStream()) {
						LoadoutViewProxy.Serialize(outputStream, new LoadoutView());

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

						//return outputStream.ToArray();
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

						//return outputStream.ToArray();
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

						//return outputStream.ToArray();
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
						ListProxy<PlayerLevelCapView>.Serialize(outputStream, new List<PlayerLevelCapView>(), PlayerLevelCapViewProxy.Serialize);

						return outputStream.ToArray();
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
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(cmid);

					using (var outputStream = new MemoryStream()) {
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
