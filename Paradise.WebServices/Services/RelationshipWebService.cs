using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Paradise.WebServices.Services {
	public class RelationshipWebService : WebServiceBase, IRelationshipWebServiceContract {
		protected override string ServiceName => "RelationshipWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IRelationshipWebServiceContract);

		public RelationshipWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() { }

		
		///
		///	Contacts = friends!
		///


		/// <summary>
		/// Accepts a friend request
		/// </summary>
		public byte[] AcceptContactRequest(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var contactRequestId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, contactRequestId);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var contactRequest = DatabaseManager.ContactRequests.FindOne(_ => _.RequestId == contactRequestId);
							var initiatorProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == contactRequest.InitiatorCmid);
							var receiverProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == contactRequest.ReceiverCmid);
								
							contactRequest.Status = ContactRequestStatus.Accepted;
							DatabaseManager.ContactRequests.Update(contactRequest);

							PublicProfileViewProxy.Serialize(outputStream, initiatorProfile);
						}

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Declines a friend request
		/// </summary>
		public byte[] DeclineContactRequest(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var contactRequestId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, contactRequestId);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var contactRequest = DatabaseManager.ContactRequests.FindOne(_ => _.RequestId == contactRequestId);

							contactRequest.Status = ContactRequestStatus.Refused;
							DatabaseManager.ContactRequests.Update(contactRequest);

							BooleanProxy.Serialize(outputStream, true);
						}

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Removes a friend from a user's friends list (TODO: confirm)
		/// </summary>
		public byte[] DeleteContact(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var contactCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, contactCmid);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							if (DatabaseManager.ContactRequests.DeleteMany(_ => ((_.ReceiverCmid == steamMember.Cmid && _.InitiatorCmid == contactCmid) || (_.InitiatorCmid == steamMember.Cmid && _.ReceiverCmid == contactCmid)) && _.Status == ContactRequestStatus.Accepted) > 0) {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
							} else {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
							}
						}

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Gets a list of pending friend requests
		/// </summary>
		public byte[] GetContactRequests(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var contactRequests = DatabaseManager.ContactRequests.Find(_ => _.ReceiverCmid == steamMember.Cmid && _.Status == ContactRequestStatus.Pending).ToList();
							ListProxy<ContactRequestView>.Serialize(outputStream, contactRequests, ContactRequestViewProxy.Serialize);
						}

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Returns a list of contacts by groups
		/// </summary>
		/// <remarks>
		/// The use of this method is currently unknown, but it's probably used to populate the friends list with clan members
		/// </remarks>
		public byte[] GetContactsByGroups(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var populateFacebookIds = BooleanProxy.Deserialize(bytes);

					DebugEndpoint(authToken, populateFacebookIds);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var contactRequests = DatabaseManager.ContactRequests.Find(_ => (_.ReceiverCmid == steamMember.Cmid || _.InitiatorCmid == steamMember.Cmid) && _.Status == ContactRequestStatus.Accepted).ToList();
							var contacts = new List<ContactGroupView>();

							foreach (ContactRequestView contactRequest in contactRequests) {
								contacts.Add(new ContactGroupView {
									GroupId = steamMember.Cmid,
									Contacts = new List<PublicProfileView> {
										DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == (contactRequest.InitiatorCmid != steamMember.Cmid ? contactRequest.InitiatorCmid : contactRequest.ReceiverCmid))
									}
								});
							}

							ListProxy<ContactGroupView>.Serialize(outputStream, contacts, ContactGroupViewProxy.Serialize);

							return outputStream.ToArray();
						}
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Sends a contact request to a user
		/// </summary>
		public byte[] SendContactRequest(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var receiverCmid = Int32Proxy.Deserialize(bytes);
					var message = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken, receiverCmid, message);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var playerProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var contactRequest = DatabaseManager.ContactRequests.FindOne(_ => (_.InitiatorCmid == steamMember.Cmid && _.ReceiverCmid == receiverCmid) || (_.InitiatorCmid == receiverCmid && _.ReceiverCmid == steamMember.Cmid));

							if (contactRequest != null) {
								contactRequest.InitiatorCmid = steamMember.Cmid;
								contactRequest.InitiatorName = playerProfile.Name;
								contactRequest.InitiatorMessage = message;
								contactRequest.ReceiverCmid = receiverCmid;
								contactRequest.SentDate = DateTime.Now;
								contactRequest.Status = ContactRequestStatus.Pending;

								DatabaseManager.ContactRequests.Update(contactRequest);
							} else {
								var r = new Random((int)DateTime.Now.Ticks);
								var requestId = r.Next(0, int.MaxValue);

								DatabaseManager.ContactRequests.Insert(new ContactRequestView {
									RequestId = requestId,
									InitiatorCmid = steamMember.Cmid,
									InitiatorName = playerProfile.Name,
									InitiatorMessage = message,
									ReceiverCmid = receiverCmid,
									SentDate = DateTime.Now
								});
							}
						}

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
