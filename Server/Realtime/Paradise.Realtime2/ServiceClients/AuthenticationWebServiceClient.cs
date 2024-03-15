using Cmune.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System.IO;
using UberStrike.Core.Serialization;
using UberStrike.Core.ViewModel;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.Realtime {
	public class AuthenticationWebServiceClient : BaseWebServiceClient<IAuthenticationWebServiceContract> {
		public static readonly AuthenticationWebServiceClient Instance;

		static AuthenticationWebServiceClient() {
			Instance = new AuthenticationWebServiceClient(
				masterHostname: BaseRealtimeApplication.Instance.Configuration.MasterHostname,
				port: BaseRealtimeApplication.Instance.Configuration.WebServicePort,
				serviceEndpoint: BaseRealtimeApplication.Instance.Configuration.WebServiceEndpoint,
				webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
				webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
			);
		}

		public AuthenticationWebServiceClient(string masterHostname, int port, string serviceEndpoint, string webServicePrefix, string webServiceSuffix) : base(masterHostname, port, serviceEndpoint, $"{webServicePrefix}AuthenticationWebService{webServiceSuffix}") { }

		public AccountCompletionResultView CompleteAccount(int cmid, string name, ChannelType channel, string locale, string machineId) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				StringProxy.Serialize(bytes, name);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, locale);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.CompleteAccount(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return AccountCompletionResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberRegistrationResult CreateUser(string emailAddress, string password, ChannelType channel, string locale, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, emailAddress);
				StringProxy.Serialize(bytes, password);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, locale);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.CreateUser(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberRegistrationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LinkSteamMember(string email, string password, string steamId, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, email);
				StringProxy.Serialize(bytes, password);
				StringProxy.Serialize(bytes, steamId);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LinkSteamMember(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LoginMemberEmail(string email, string password, ChannelType channel, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, email);
				StringProxy.Serialize(bytes, password);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LoginMemberEmail(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LoginMemberFacebookUnitySdk(string facebookPlayerAcccessToken, ChannelType channel, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, facebookPlayerAcccessToken);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LoginMemberFacebookUnitySdk(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LoginMemberPortal(int cmid, string hash, string machineId) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				StringProxy.Serialize(bytes, hash);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LoginMemberPortal(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LoginSteam(string steamId, string authToken, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, steamId);
				StringProxy.Serialize(bytes, authToken);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LoginSteam(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView VerifyAuthToken(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.VerifyAuthToken(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}
	}
}
