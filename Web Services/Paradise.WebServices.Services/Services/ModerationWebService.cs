using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class ModerationWebService : BaseWebService, IModerationWebServiceContract {
		protected static readonly new ILog Log = LogManager.GetLogger(nameof(ModerationWebService));

		public override string ServiceName => "ModerationWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IModerationWebServiceContract);

		public ModerationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		#region IModerationWebServiceContract
		/// <summary>
		/// Bans a user by Cmid permanently
		/// This method is actually unused by the game and
		/// superseded by sending ban commands through the service socket
		/// </summary>
		/// 
		public byte[] BanPermanently(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, targetCmid);

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

		public byte[] SetModerationFlag(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var moderationFlag = EnumProxy<ModerationFlag>.Deserialize(bytes);
					var expireTime = DateTimeProxy.Deserialize(bytes);
					var reason = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken, targetCmid, moderationFlag, expireTime, reason);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (publicProfile == null || publicProfile.AccessLevel < MemberAccessLevel.Moderator) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								} else {
									var targetProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

									if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
									} else if (targetProfile.AccessLevel < publicProfile.AccessLevel) {
										if (DatabaseClient.ModerationActions.FindOne(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag) is var moderationAction && moderationAction != null) {
											moderationAction.ActionDate = DateTime.UtcNow;
											moderationAction.ExpireTime = expireTime;
											moderationAction.SourceCmid = publicProfile.Cmid;
											moderationAction.SourceName = publicProfile.Name;

											DatabaseClient.ModerationActions.DeleteMany(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag);
											DatabaseClient.ModerationActions.Insert(moderationAction);
										} else {
											DatabaseClient.ModerationActions.Insert(new ModerationAction {
												ActionDate = DateTime.UtcNow,
												ExpireTime = expireTime,
												ModerationFlag = moderationFlag,
												Reason = reason,
												SourceCmid = publicProfile.Cmid,
												SourceName = publicProfile.Name,
												TargetCmid = targetProfile.Cmid,
												TargetName = targetProfile.Name
											});
										}

										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
									} else {
										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
									}
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

		public byte[] UnsetModerationFlag(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var moderationFlag = EnumProxy<ModerationFlag>.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (publicProfile == null || publicProfile.AccessLevel < MemberAccessLevel.Moderator) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								} else {
									var targetProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

									if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
									} else if (targetProfile.AccessLevel < publicProfile.AccessLevel) {
										if (DatabaseClient.ModerationActions.FindOne(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag) is var moderationAction && moderationAction != null) {
											//DatabaseClient.ModerationActions.DeleteMany(_ => _.TargetCmid == targetProfile.Cmid && _.ModerationFlag == moderationFlag);
											moderationAction.ExpireTime = DateTime.MinValue;

											DatabaseClient.ModerationActions.DeleteMany(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag);
											DatabaseClient.ModerationActions.Insert(moderationAction);

											EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
										} else {
											EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
										}
									} else {
										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
									}
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

		public byte[] ClearModerationFlags(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (publicProfile == null || publicProfile.AccessLevel < MemberAccessLevel.Moderator) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								} else {
									var targetProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

									if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
									} else if (targetProfile.AccessLevel < publicProfile.AccessLevel) {
										if (DatabaseClient.ModerationActions.Find(_ => _.TargetCmid == targetCmid) is var moderationActions) {
											//DatabaseClient.ModerationActions.DeleteMany(_ => _.TargetCmid == targetProfile.Cmid);
											foreach (var action in moderationActions) {
												action.ExpireTime = DateTime.MinValue;

												DatabaseClient.ModerationActions.DeleteMany(_ => _.TargetCmid == targetCmid && _.ModerationFlag == action.ModerationFlag);
												DatabaseClient.ModerationActions.Insert(action);
											}

											EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
										} else {
											EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
										}
									} else {
										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
									}
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

		public byte[] GetNaughtyList(byte[] data) {
			try {
				var isEncrypted = IsEncrypted(data);

				using (var bytes = new MemoryStream(isEncrypted ? CryptoPolicy.RijndaelDecrypt(data, EncryptionPassPhrase, EncryptionInitVector) : data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(System.Reflection.MethodBase.GetCurrentMethod(), authToken);

					using (var outputStream = new MemoryStream()) {
						if (GameSessionManager.Instance.TryGetValue(authToken, out var session)) {
							var steamMember = session.SteamMember;

							if (steamMember != null) {
								var publicProfile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

								if (publicProfile == null || publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
									var naughtyUsers = new List<CommActorInfo>();
									var moderationActions = DatabaseClient.ModerationActions.Find(_ => _.ExpireTime == null || _.ExpireTime > DateTime.UtcNow);

									foreach (var action in moderationActions) {
										if (naughtyUsers.Find(_ => _.Cmid.Equals(action.TargetCmid)) is var user && user != null) {
											user.ModerationFlag |= (byte)action.ModerationFlag;
										} else {
											var profile = DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == action.TargetCmid);
											var clan = DatabaseClient.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid.Equals(action.TargetCmid)) != null);

											naughtyUsers.Add(new CommActorInfo {
												AccessLevel = profile.AccessLevel,
												Channel = ChannelType.Steam,
												ClanTag = clan?.Tag,
												Cmid = action.TargetCmid,
												ModerationFlag = (byte)action.ModerationFlag,
												ModInformation = action.Reason,
												PlayerName = profile.Name
											});
										}
									}

									ListProxy<CommActorInfo>.Serialize(outputStream, naughtyUsers, CommActorInfoProxy.Serialize);
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
