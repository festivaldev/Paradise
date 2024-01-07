using Cmune.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;
using UberStrike.Core.Serialization;

namespace Paradise.Realtime {
	class ClanWebServiceClient : BaseWebServiceClient<IClanWebServiceContract> {
		public static readonly ClanWebServiceClient Instance;

		static ClanWebServiceClient() {
			Instance = new ClanWebServiceClient(
				masterUrl: BaseRealtimeApplication.Instance.Configuration.MasterServerUrl,
				port: BaseRealtimeApplication.Instance.Configuration.WebServicePort,
				serviceEndpoint: BaseRealtimeApplication.Instance.Configuration.WebServiceEndpoint,
				webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
				webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
			);
		}

		public ClanWebServiceClient(string masterUrl, int port, string serviceEndpoint, string webServicePrefix, string webServiceSuffix) : base(masterUrl, port, serviceEndpoint, $"{webServicePrefix}ClanWebService{webServiceSuffix}") { }

		public ClanRequestAcceptView AcceptClanInvitation(int clanInvitationId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, clanInvitationId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.AcceptClanInvitation(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ClanRequestAcceptViewProxy.Deserialize(inputStream);
				}
			}
		}

		public int CancelInvitation(int groupInvitationId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupInvitationId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.CancelInvitation(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int CanOwnAClan(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.CanOwnAClan(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public ClanCreationReturnView CreateClan(GroupCreationView createClanData) {
			using (var bytes = new MemoryStream()) {
				GroupCreationViewProxy.Serialize(bytes, createClanData);

				var result = Service.CreateClan(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ClanCreationReturnViewProxy.Deserialize(inputStream);
				}
			}
		}

		public ClanRequestDeclineView DeclineClanInvitation(int clanInvitationId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, clanInvitationId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DeclineClanInvitation(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ClanRequestDeclineViewProxy.Deserialize(inputStream);
				}
			}
		}

		public int DisbandGroup(int groupId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DisbandGroup(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public List<GroupInvitationView> GetAllGroupInvitations(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetAllGroupInvitations(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<GroupInvitationView>.Deserialize(inputStream, GroupInvitationViewProxy.Deserialize);
				}
			}
		}

		public int GetMyClanId(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMyClanId(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public ClanView GetOwnClan(string authToken, int groupId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, groupId);

				var result = Service.GetOwnClan(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ClanViewProxy.Deserialize(inputStream);
				}
			}
		}

		public List<GroupInvitationView> GetPendingGroupInvitations(int groupId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetPendingGroupInvitations(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<GroupInvitationView>.Deserialize(inputStream, GroupInvitationViewProxy.Deserialize);
				}
			}
		}

		public int InviteMemberToJoinAGroup(int clanId, string authToken, int inviteeCmid, string message) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, clanId);
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, inviteeCmid);
				StringProxy.Serialize(bytes, message);

				var result = Service.InviteMemberToJoinAGroup(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int KickMemberFromClan(int groupId, string authToken, int cmidToKick) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, cmidToKick);

				var result = Service.KickMemberFromClan(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int LeaveAClan(int groupId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.LeaveAClan(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int TransferOwnership(int groupId, string authToken, int newLeaderCmid) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, newLeaderCmid);

				var result = Service.TransferOwnership(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int UpdateMemberPosition(MemberPositionUpdateView updateMemberPositionData) {
			using (var bytes = new MemoryStream()) {
				MemberPositionUpdateViewProxy.Serialize(bytes, updateMemberPositionData);

				var result = Service.UpdateMemberPosition(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}
	}
}
