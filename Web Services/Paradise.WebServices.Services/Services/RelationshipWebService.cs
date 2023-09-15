using log4net;
using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class RelationshipWebService : BaseWebService, IRelationshipWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(RelationshipWebService));

		public override string ServiceName => "RelationshipWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IRelationshipWebServiceContract);

		public RelationshipWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		///
		///	Contacts = friends!
		///

		#region IRelationshipWebServiceContract
		/// <summary>
		/// Accepts a friend request
		/// </summary>
		public byte[] AcceptContactRequest(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var contactRequestId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, contactRequestId);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var contactRequest = DatabaseClient.ContactRequests.FindOne(_ => _.RequestId == contactRequestId);
								var initiatorProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == contactRequest.InitiatorCmid);
								var receiverProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == contactRequest.ReceiverCmid);

								contactRequest.Status = ContactRequestStatus.Accepted;
								DatabaseClient.ContactRequests.Update(contactRequest);

								PublicProfileViewProxy.Serialize(outputStream, initiatorProfile);
							}
						}

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

		/// <summary>
		/// Declines a friend request
		/// </summary>
		public byte[] DeclineContactRequest(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var contactRequestId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, contactRequestId);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var contactRequest = DatabaseClient.ContactRequests.FindOne(_ => _.RequestId == contactRequestId);

								contactRequest.Status = ContactRequestStatus.Refused;
								DatabaseClient.ContactRequests.Update(contactRequest);

								BooleanProxy.Serialize(outputStream, true);
							}
						}

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

		/// <summary>
		/// Removes a friend from a user's friends list (TODO: confirm)
		/// </summary>
		public byte[] DeleteContact(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var contactCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, contactCmid);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								if (DatabaseClient.ContactRequests.DeleteMany(_ => ((_.ReceiverCmid == steamMember.Cmid && _.InitiatorCmid == contactCmid) || (_.InitiatorCmid == steamMember.Cmid && _.ReceiverCmid == contactCmid)) && _.Status == ContactRequestStatus.Accepted) > 0) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
								} else {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
								}
							}
						}

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

		/// <summary>
		/// Gets a list of pending friend requests
		/// </summary>
		public byte[] GetContactRequests(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var contactRequests = DatabaseClient.ContactRequests.Find(_ => _.ReceiverCmid == steamMember.Cmid && _.Status == ContactRequestStatus.Pending).ToList();

								ListProxy<ContactRequestView>.Serialize(outputStream, contactRequests, ContactRequestViewProxy.Serialize);
							}
						}

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

		/// <summary>
		/// Returns a list of contacts by groups
		/// </summary>
		/// <remarks>
		/// The use of this method is currently unknown, but it's probably used to populate the friends list with clan members
		/// </remarks>
		public byte[] GetContactsByGroups(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var populateFacebookIds = BooleanProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, populateFacebookIds);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var contactRequests = DatabaseClient.ContactRequests.Find(_ => (_.ReceiverCmid == steamMember.Cmid || _.InitiatorCmid == steamMember.Cmid) && _.Status == ContactRequestStatus.Accepted).ToList();
								var contacts = new List<ContactGroupView>();

								foreach (ContactRequestView contactRequest in contactRequests) {
									contacts.Add(new ContactGroupView {
										GroupId = steamMember.Cmid,
										Contacts = new List<PublicProfileView> {
										DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == (contactRequest.InitiatorCmid != steamMember.Cmid ? contactRequest.InitiatorCmid : contactRequest.ReceiverCmid))
									}
									});
								}

								ListProxy<ContactGroupView>.Serialize(outputStream, contacts, ContactGroupViewProxy.Serialize);
							}
						}

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

		/// <summary>
		/// Sends a contact request to a user
		/// </summary>
		public byte[] SendContactRequest(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var receiverCmid = Int32Proxy.Deserialize(bytes);
					var message = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, receiverCmid, message);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var playerProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
								var contactRequest = DatabaseClient.ContactRequests.FindOne(_ => (_.InitiatorCmid == steamMember.Cmid && _.ReceiverCmid == receiverCmid) || (_.InitiatorCmid == receiverCmid && _.ReceiverCmid == steamMember.Cmid));

								if (contactRequest != null) {
									contactRequest.InitiatorCmid = steamMember.Cmid;
									contactRequest.InitiatorName = playerProfile.Name;
									contactRequest.InitiatorMessage = message;
									contactRequest.ReceiverCmid = receiverCmid;
									contactRequest.SentDate = DateTime.UtcNow;
									contactRequest.Status = ContactRequestStatus.Pending;

									DatabaseClient.ContactRequests.Update(contactRequest);
								} else {
									var r = new Random((int)DateTime.UtcNow.Ticks);
									var requestId = r.Next(0, int.MaxValue);

									DatabaseClient.ContactRequests.Insert(new ContactRequestView {
										RequestId = requestId,
										InitiatorCmid = steamMember.Cmid,
										InitiatorName = playerProfile.Name,
										InitiatorMessage = message,
										ReceiverCmid = receiverCmid,
										SentDate = DateTime.UtcNow
									});
								}
							}
						}

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
		#endregion
	}
}
