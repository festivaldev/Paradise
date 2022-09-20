using Paradise.Core.Serialization;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System.IO;

namespace Paradise.Realtime {
	public class AuthenticationWebServiceClient : BaseWebServiceClient<IAuthenticationWebServiceContract> {
		public static readonly AuthenticationWebServiceClient Instance = new AuthenticationWebServiceClient(
			endpointUrl: BaseRealtimeApplication.Instance.Configuration.WebServiceBaseUrl,
			webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
			webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
		);

		public AuthenticationWebServiceClient(string endpointUrl, string webServicePrefix, string webServiceSuffix) : base(endpointUrl, $"{webServicePrefix}AuthenticationWebService{webServiceSuffix}") { }

		public AccountCompletionResultView CompleteAccount(int cmid, string name, ChannelType channel, string locale, string machineId) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				StringProxy.Serialize(bytes, name);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, locale);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.CompleteAccount(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
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

				var result = Service.CreateUser(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
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

				var result = Service.LinkSteamMember(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
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

				var result = Service.LoginMemberEmail(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LoginMemberFacebookUnitySdk(string facebookPlayerAcccessToken, ChannelType channel, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, facebookPlayerAcccessToken);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LoginMemberFacebookUnitySdk(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LoginMemberPortal(int cmid, string hash, string machineId) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				StringProxy.Serialize(bytes, hash);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LoginMemberPortal(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberAuthenticationResultView LoginSteam(string steamId, string authToken, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, steamId);
				StringProxy.Serialize(bytes, authToken);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.LoginSteam(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MemberAuthenticationResultViewProxy.Deserialize(inputStream);
				}
			}
		}
	}
}
