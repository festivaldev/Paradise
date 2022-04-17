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
	public class AuthenticationWebService_Legacy : WebServiceBase, IAuthenticationWebServiceContract_Legacy {
		public override string ServiceName => "AuthenticationWebService";
		public override string ServiceVersion => "1.0.1";
		protected override Type ServiceInterface => typeof(IAuthenticationWebServiceContract_Legacy);

		public AuthenticationWebService_Legacy(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() {}

		public byte[] CompleteAccount(byte[] data) {
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

		public byte[] CreateUser(byte[] data) {
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

		public byte[] LoginMemberCookie(byte[] data) {
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

		public byte[] LoginMemberEmail(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					DebugEndpoint();

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
							ServerTime = DateTime.Now
						});

						
						return CryptoPolicy.RijndaelEncrypt(outputStream.ToArray(), "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=", "aaaabbbbccccdddd");
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] UncompleteAccount(byte[] data) {
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