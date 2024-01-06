using Cmune.DataCenter.Common.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using UberStrike.Core.Serialization.Legacy;

namespace Paradise.WebServices.LegacyServices._102 {
	public class PrivateMessageWebService : BaseWebService, IPrivateMessageWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(PrivateMessageWebService));

		public override string ServiceName => "PrivateMessageWebService";
		public override string ServiceVersion => ApiVersion.Legacy102;
		protected override Type ServiceInterface => typeof(IPrivateMessageWebServiceContract);

		public PrivateMessageWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		#region IPrivateMessageWebServiceContract
		public byte[] GetAllMessageThreadsForUser_1(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid);

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

		public byte[] GetAllMessageThreadsForUser_2(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var cmid = Int32Proxy.Deserialize(bytes);
					var pageNumber = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), cmid, pageNumber);

					using (var outputStream = new MemoryStream()) {
						var userAccount = DatabaseClient.UserAccounts.FindOne(_ => _.Cmid.Equals(cmid));

						if (userAccount != null) {
							var messages = DatabaseClient.PrivateMessages.Find(_ => _.FromCmid == userAccount.Cmid || _.ToCmid == userAccount.Cmid).GroupBy(_ => {
								var threadId = new List<int> { _.FromCmid, _.ToCmid };
								threadId.Sort();
								return string.Join(",", threadId);
							}).ToDictionary(_ => _.Key, _ => _.ToList());

							List<MessageThreadView> threads = new List<MessageThreadView>();

							foreach (KeyValuePair<string, List<PrivateMessageView>> messageGroup in messages) {
								var filteredMessages = messageGroup.Value.FindAll(_ => (_.FromCmid == userAccount.Cmid && !_.IsDeletedBySender) || (_.ToCmid == userAccount.Cmid && !_.IsDeletedByReceiver));

								if (filteredMessages.Count > 0) {
									var message = filteredMessages.Last();

									var otherCmid = message.FromCmid != userAccount.Cmid ? message.FromCmid : message.ToCmid;
									var otherProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == otherCmid);

									if (otherProfile != null) {
										threads.Add(new MessageThreadView {
											ThreadId = otherCmid,
											ThreadName = otherProfile.Name,
											MessageCount = filteredMessages.Count(),
											LastMessagePreview = message.ContentText,
											LastUpdate = message.DateSent,
											HasNewMessages = filteredMessages.Any(_ => _.ToCmid == userAccount.Cmid && !_.IsRead)
										});
									}
								}
							}

							ListProxy<MessageThreadView>.Serialize(outputStream, threads, MessageThreadViewProxy.Serialize);
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

		public byte[] GetThreadMessages(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var threadViewerCmid = Int32Proxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);
					var pageNumber = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), threadViewerCmid, otherCmid, pageNumber);

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

		public byte[] SendMessage(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var senderCmid = Int32Proxy.Deserialize(bytes);
					var receiverCmid = Int32Proxy.Deserialize(bytes);
					var content = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), senderCmid, receiverCmid, content);

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

		public byte[] GetMessageWithId(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var messageId = Int32Proxy.Deserialize(bytes);
					var requesterCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), messageId, requesterCmid);

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

		public byte[] MarkThreadAsRead(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var threadViewerCmid = Int32Proxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), threadViewerCmid, otherCmid);

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

		public byte[] DeleteThread(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var threadViewerCmid = Int32Proxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), threadViewerCmid, otherCmid);

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
