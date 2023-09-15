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
	public class PrivateMessageWebService : BaseWebService, IPrivateMessageWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(PrivateMessageWebService));

		public override string ServiceName => "PrivateMessageWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IPrivateMessageWebServiceContract);

		public PrivateMessageWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		#region IPrivateMessageWebServiceContract
		/// <summary>
		/// Deletes a specified message thread with a different user
		/// </summary>
		public byte[] DeleteThread(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, otherCmid);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var messages = DatabaseClient.PrivateMessages.Find(_ => (_.FromCmid == steamMember.Cmid && _.ToCmid == otherCmid) || (_.FromCmid == otherCmid && _.ToCmid == steamMember.Cmid));

								foreach (var message in messages) {
									if (message.FromCmid == steamMember.Cmid) {
										message.IsDeletedBySender = true;
									} else if (message.ToCmid == steamMember.Cmid) {
										message.IsDeletedByReceiver = true;
									}

									DatabaseClient.PrivateMessages.Update(message);
								}

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
		/// Returns a list of message threads for a user
		/// </summary>
		public byte[] GetAllMessageThreadsForUser(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageNumber = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, pageNumber);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var messages = DatabaseClient.PrivateMessages.Find(_ => _.FromCmid == steamMember.Cmid || _.ToCmid == steamMember.Cmid).GroupBy(_ => {
									var threadId = new List<int> { _.FromCmid, _.ToCmid };
									threadId.Sort();
									return string.Join(",", threadId);
								}).ToDictionary(_ => _.Key, _ => _.ToList());

								List<MessageThreadView> threads = new List<MessageThreadView>();

								foreach (KeyValuePair<string, List<PrivateMessageView>> messageGroup in messages) {
									var filteredMessages = messageGroup.Value.FindAll(_ => (_.FromCmid == steamMember.Cmid && !_.IsDeletedBySender) || (_.ToCmid == steamMember.Cmid && !_.IsDeletedByReceiver));

									if (filteredMessages.Count > 0) {
										var message = filteredMessages.Last();

										var otherCmid = message.FromCmid != steamMember.Cmid ? message.FromCmid : message.ToCmid;
										var otherProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == otherCmid);

										if (otherProfile != null) {
											threads.Add(new MessageThreadView {
												ThreadId = otherCmid,
												ThreadName = otherProfile.Name,
												MessageCount = filteredMessages.Count(),
												LastMessagePreview = message.ContentText,
												LastUpdate = message.DateSent,
												HasNewMessages = filteredMessages.Any(_ => _.ToCmid == steamMember.Cmid && !_.IsRead)
											});
										}
									}
								}

								ListProxy<MessageThreadView>.Serialize(outputStream, threads, MessageThreadViewProxy.Serialize);
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
		/// Returns a single message for a specified ID
		/// </summary>
		public byte[] GetMessageWithIdForCmid(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var messageId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, messageId);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var message = DatabaseClient.PrivateMessages.FindOne(_ => _.PrivateMessageId == messageId && ((_.FromCmid == steamMember.Cmid && !_.IsDeletedBySender) || (_.ToCmid == steamMember.Cmid && !_.IsDeletedByReceiver)));

								PrivateMessageViewProxy.Serialize(outputStream, message);
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
		/// Returns a list of messages in a thread
		/// </summary>
		public byte[] GetThreadMessages(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);
					var pageNumber = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, otherCmid, pageNumber);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var messages = DatabaseClient.PrivateMessages.Find(_ => (_.FromCmid == steamMember.Cmid && _.ToCmid == otherCmid && !_.IsDeletedBySender) || (_.FromCmid == otherCmid && _.ToCmid == steamMember.Cmid && !_.IsDeletedByReceiver));

								ListProxy<PrivateMessageView>.Serialize(outputStream, messages.ToList(), PrivateMessageViewProxy.Serialize);
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
		/// Marks a thread as read
		/// </summary>
		public byte[] MarkThreadAsRead(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, otherCmid);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var messages = DatabaseClient.PrivateMessages.Find(_ => _.ToCmid == steamMember.Cmid && _.FromCmid == otherCmid && !_.IsRead);

								foreach (var message in messages) {
									message.IsRead = true;
									DatabaseClient.PrivateMessages.Update(message);
								}

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
		/// Sends a message to another user
		/// </summary>
		public byte[] SendMessage(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var receiverCmid = Int32Proxy.Deserialize(bytes);
					var content = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, receiverCmid, content);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var sender = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
								var receiver = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == receiverCmid);

								var r = new Random((int)DateTime.UtcNow.Ticks);

								var privateMessage = new PrivateMessageView {
									PrivateMessageId = DatabaseClient.PrivateMessages.Count() + 1,
									FromCmid = sender.Cmid,
									FromName = sender.Name,
									ToCmid = receiverCmid,
									DateSent = DateTime.UtcNow,
									ContentText = content,
									IsRead = false
								};

								DatabaseClient.PrivateMessages.Insert(privateMessage);

								var participants = new List<int>() { sender.Cmid, receiver.Cmid };
								participants.Sort();

								PrivateMessageViewProxy.Serialize(outputStream, privateMessage);
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
