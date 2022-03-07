using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;

namespace Paradise.WebServices.Client {
	class ClanWebServiceClient : WebServiceClientBase<IClanWebServiceContract> {
		public ClanWebServiceClient(string endpointUrl) : base(endpointUrl, $"{Properties.Resources.WebServicePrefix}ClanWebService{Properties.Resources.WebServiceSuffix}") { }

		public ClanRequestAcceptView AcceptClanInvitation(int clanInvitationId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, clanInvitationId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.AcceptClanInvitation(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ClanRequestAcceptViewProxy.Deserialize(inputStream);
				}
			}
		}

		public int CancelInvitation(int groupInvitationId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupInvitationId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.CancelInvitation(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int CanOwnAClan(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.CanOwnAClan(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public ClanCreationReturnView CreateClan(GroupCreationView createClanData) {
			using (var bytes = new MemoryStream()) {
				GroupCreationViewProxy.Serialize(bytes, createClanData);

				var result = Service.CreateClan(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ClanCreationReturnViewProxy.Deserialize(inputStream);
				}
			}
		}

		public ClanRequestDeclineView DeclineClanInvitation(int clanInvitationId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, clanInvitationId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DeclineClanInvitation(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ClanRequestDeclineViewProxy.Deserialize(inputStream);
				}
			}
		}

		public int DisbandGroup(int groupId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DisbandGroup(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public List<GroupInvitationView> GetAllGroupInvitations(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetAllGroupInvitations(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<GroupInvitationView>.Deserialize(inputStream, GroupInvitationViewProxy.Deserialize);
				}
			}
		}

		public int GetMyClanId(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMyClanId(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public ClanView GetOwnClan(string authToken, int groupId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, groupId);

				var result = Service.GetOwnClan(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ClanViewProxy.Deserialize(inputStream);
				}
			}
		}

		public List<GroupInvitationView> GetPendingGroupInvitations(int groupId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetPendingGroupInvitations(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
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

				var result = Service.InviteMemberToJoinAGroup(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int KickMemberFromClan(int groupId, string authToken, int cmidToKick) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, cmidToKick);

				var result = Service.KickMemberFromClan(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int LeaveAClan(int groupId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.LeaveAClan(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int TransferOwnership(int groupId, string authToken, int newLeaderCmid) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, groupId);
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, newLeaderCmid);

				var result = Service.TransferOwnership(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int UpdateMemberPosition(MemberPositionUpdateView updateMemberPositionData) {
			using (var bytes = new MemoryStream()) {
				MemberPositionUpdateViewProxy.Serialize(bytes, updateMemberPositionData);

				var result = Service.UpdateMemberPosition(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}
	}
}
