using Cmune.DataCenter.Common.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using UberStrike.Core.Serialization.Legacy;

namespace Paradise.WebServices.LegacyServices._102 {
	public class RelationshipWebService : BaseWebService, IRelationshipWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(RelationshipWebService));

		public override string ServiceName => "RelationshipWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IRelationshipWebServiceContract);

		public RelationshipWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		///
		///	Contacts = friends!
		///

		#region IRelationshipWebServiceContract
		public byte[] SendContactRequest(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var initiatorCmid = Int32Proxy.Deserialize(bytes);
					var receiverCmid = Int32Proxy.Deserialize(bytes);
					var message = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), initiatorCmid, receiverCmid, message);

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

		public byte[] GetContactRequests(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid);

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(cmid));

						if (userAccount != null) {
							var contactRequests = DatabaseClient.ContactRequests.Find(_ => _.ReceiverCmid == userAccount.Cmid && _.Status == ContactRequestStatus.Pending).ToList();

							ListProxy<ContactRequestView>.Serialize(outputStream, contactRequests, ContactRequestViewProxy.Serialize);
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

		public byte[] AcceptContactRequest(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var contactRequestId = Int32Proxy.Deserialize(bytes);
					var cmid = Int32Proxy.Deserialize(bytes);
					var applicationId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), contactRequestId, cmid, applicationId);

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

		public byte[] DeclineContactRequest(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var contactRequestId = Int32Proxy.Deserialize(bytes);
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), contactRequestId, cmid);

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

		public byte[] DeleteContact(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var contactCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, contactCmid);

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

		public byte[] MoveContactToGroup(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var contactCmid = Int32Proxy.Deserialize(bytes);
					var previousGroupId = Int32Proxy.Deserialize(bytes);
					var newGroupId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, contactCmid, previousGroupId, newGroupId);

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

		public byte[] GetContactsByGroups(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var applicationId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod());

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid == cmid);

						if (userAccount != null) {
							var contactRequests = DatabaseClient.ContactRequests.Find(_ => (_.ReceiverCmid == userAccount.Cmid || _.InitiatorCmid == userAccount.Cmid) && _.Status == ContactRequestStatus.Accepted).ToList();
							var contacts = new List<ContactGroupView>();

							foreach (ContactRequestView contactRequest in contactRequests) {
								contacts.Add(new ContactGroupView {
									GroupId = userAccount.Cmid,
									GroupName = "DEBUG TEST GROUP",
									Contacts = new List<PublicProfileView> {
										DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == (contactRequest.InitiatorCmid != userAccount.Cmid ? contactRequest.InitiatorCmid : contactRequest.ReceiverCmid))
									}
								});
							}

							ListProxy<ContactGroupView>.Serialize(outputStream, contacts, ContactGroupViewProxy.Serialize);
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
