using log4net;
using Paradise.Core.Serialization.Legacy;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Paradise.WebServices.LegacyServices._102 {
	public class AuthenticationWebService : BaseWebService, IAuthenticationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(AuthenticationWebService));

		public override string ServiceName => "AuthenticationWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IAuthenticationWebServiceContract);

		//private static ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

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

		public byte[] CompleteAccount(byte[] data) {
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

		public byte[] LoginMemberFacebook(byte[] data) {
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

		public byte[] FacebookSingleSignOn(byte[] data) {
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
		#endregion
	}
}
