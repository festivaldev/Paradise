using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class PrivateMessageWebService : WebServiceBase, IPrivateMessageWebServiceContract {
		protected override string ServiceName => "PrivateMessageWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IPrivateMessageWebServiceContract);

		public PrivateMessageWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }

		protected override void Setup() { }

		/// <summary>
		/// Deletes a specified message thread with a different user
		/// </summary>
		public byte[] DeleteThread(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, otherCmid);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var messages = DatabaseManager.PrivateMessages.Find(_ => (_.FromCmid == steamMember.Cmid && _.ToCmid == otherCmid) || (_.FromCmid == otherCmid && _.ToCmid == steamMember.Cmid));

							foreach (var message in messages) {
								if (message.FromCmid == steamMember.Cmid) {
									message.IsDeletedBySender = true;
								} else if (message.ToCmid == steamMember.Cmid) {
									message.IsDeletedByReceiver = true;
								}

								DatabaseManager.PrivateMessages.Update(message);
							}

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
		/// Returns a list of message threads for a user
		/// </summary>
		public byte[] GetAllMessageThreadsForUser(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var pageNumber = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, pageNumber);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var messages = DatabaseManager.PrivateMessages.Find(_ => _.FromCmid == steamMember.Cmid || _.ToCmid == steamMember.Cmid).GroupBy(_ => {
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
									var otherProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == otherCmid);

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

						return outputStream.ToArray();
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
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var messageId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, messageId);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var message = DatabaseManager.PrivateMessages.FindOne(_ => _.PrivateMessageId == messageId && ((_.FromCmid == steamMember.Cmid && !_.IsDeletedBySender) || (_.ToCmid == steamMember.Cmid && !_.IsDeletedByReceiver)));

							PrivateMessageViewProxy.Serialize(outputStream, message);
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
		/// Returns a list of messages in a thread
		/// </summary>
		public byte[] GetThreadMessages(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);
					var pageNumber = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, otherCmid, pageNumber);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var messages = DatabaseManager.PrivateMessages.Find(_ => (_.FromCmid == steamMember.Cmid && _.ToCmid == otherCmid && !_.IsDeletedBySender) || (_.FromCmid == otherCmid && _.ToCmid == steamMember.Cmid && !_.IsDeletedByReceiver));

							ListProxy<PrivateMessageView>.Serialize(outputStream, messages.ToList(), PrivateMessageViewProxy.Serialize);
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
		/// Marks a thread as read
		/// </summary>
		public byte[] MarkThreadAsRead(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var otherCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, otherCmid);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var messages = DatabaseManager.PrivateMessages.Find(_ => _.ToCmid == steamMember.Cmid && _.FromCmid == otherCmid && !_.IsRead);

							foreach (var message in messages) {
								message.IsRead = true;
								DatabaseManager.PrivateMessages.Update(message);
							}

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
		/// Sends a message to another user
		/// </summary>
		public byte[] SendMessage(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var receiverCmid = Int32Proxy.Deserialize(bytes);
					var content = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken, receiverCmid, content);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var sender = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var receiver = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == receiverCmid);

							var r = new Random((int)DateTime.Now.Ticks);

							var privateMessage = new PrivateMessageView {
								PrivateMessageId = DatabaseManager.PrivateMessages.Count() + 1,
								FromCmid = sender.Cmid,
								FromName = sender.Name,
								ToCmid = receiverCmid,
								DateSent = DateTime.Now,
								ContentText = content,
								IsRead = false
							};

							DatabaseManager.PrivateMessages.Insert(privateMessage);

							var participants = new List<int>() { sender.Cmid, receiver.Cmid };
							participants.Sort();

							PrivateMessageViewProxy.Serialize(outputStream, privateMessage);
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
