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
	public class AuthenticationWebService : BaseWebService, IAuthenticationWebServiceContract {
		public override string ServiceName => "AuthenticationWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IAuthenticationWebServiceContract);


		public AuthenticationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() {
			
		}

		protected override void Teardown() {

		}



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
						EnumProxy<MemberRegistrationResult>.Serialize(outputStream, MemberRegistrationResult.Ok);

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] CompleteAccount(byte[] data) {
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

		public byte[] LoginMemberEmail(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var emailAddress = StringProxy.Deserialize(bytes);
					var password = StringProxy.Deserialize(bytes);
					var channel = EnumProxy<ChannelType>.Deserialize(bytes);
					var machineId = StringProxy.Deserialize(bytes);

					DebugEndpoint(emailAddress, password, channel, machineId);

					using (var outputStream = new MemoryStream()) {
						MemberAuthenticationResultViewProxy.Serialize(outputStream, new MemberAuthenticationResultView { 
							IsAccountComplete = true,
							IsTutorialComplete = true,
							MemberAuthenticationResult = MemberAuthenticationResult.Ok,
							MemberView = new MemberView {
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
								}
							},
							PlayerStatisticsView = new PlayerStatisticsView { 
								Cmid = 1,
								Xp = 1000
							},
							WeeklySpecial = new WeeklySpecialView {
								StartDate = DateTime.MinValue,
								EndDate = DateTime.MaxValue,
								Id = 0,
								ImageUrl = "http://via.placeholder.com/350x150",
								Text = "LockWatch 2 Beta (iOS 13/14)",
								Title = "Team FESTIVAL",
								ItemId = 1003
							},
							ServerTime = DateTime.UtcNow
						});

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] LoginMemberCookie(byte[] data) {
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

		public byte[] LoginMemberFacebook(byte[] data) {
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

		public byte[] FacebookSingleSignOn(byte[] data) {
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
	}
}
