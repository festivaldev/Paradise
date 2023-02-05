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
		public override string ServiceName => "ModerationWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IModerationWebServiceContract);

		public ModerationWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public ModerationWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		public byte[] OpPlayer(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var accessLevel = EnumProxy<MemberAccessLevel>.Deserialize(bytes);

					DebugEndpoint(authToken, targetCmid, accessLevel);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

							if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
								var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
									return outputStream.ToArray();
								}

								if (accessLevel == MemberAccessLevel.Default) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
									return outputStream.ToArray();
								}

								if (!Enum.IsDefined(typeof(MemberAccessLevel), accessLevel)) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
									return outputStream.ToArray();
								}

								if (targetProfile.AccessLevel < publicProfile.AccessLevel &&
									accessLevel < publicProfile.AccessLevel &&
									accessLevel > targetProfile.AccessLevel) {
									targetProfile.AccessLevel = accessLevel;

									DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
									DatabaseManager.PublicProfiles.Insert(targetProfile);

									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
								} else {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								}
							}
						} else {
							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
						}

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] DeopPlayer(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, targetCmid);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

							if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
								var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
									return outputStream.ToArray();
								}

								if (targetProfile != null &&
									targetProfile.Cmid != publicProfile.Cmid &&
									targetProfile.AccessLevel > MemberAccessLevel.Default &&
									targetProfile.AccessLevel < publicProfile.AccessLevel) {
									targetProfile.AccessLevel = MemberAccessLevel.Default;

									DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
									DatabaseManager.PublicProfiles.Insert(targetProfile);

									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
								} else {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								}
							}
						} else {
							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
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
		/// Bans a user by Cmid permanently
		/// </summary>
		//public byte[] BanPermanently(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var targetCmid = Int32Proxy.Deserialize(bytes);
		//			var reason = StringProxy.Deserialize(bytes);

		//			DebugEndpoint(authToken, targetCmid, reason);

		//			using (var outputStream = new MemoryStream()) {
		//				var steamMember = SteamMemberFromAuthToken(authToken);

		//				if (steamMember != null) {
		//					var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

		//					if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
		//						var profileToBan = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

		//						if (profileToBan == null || profileToBan.Cmid == publicProfile.Cmid) {
		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
		//							return outputStream.ToArray();
		//						}

		//						if (profileToBan.AccessLevel < publicProfile.AccessLevel) {
		//							if (DatabaseManager.ModerationActions.FindOne(_ => _.ActionType == ModerationActionType.AccountPermanentBan && _.TargetCmid == profileToBan.Cmid) != null) {
		//								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
		//							} else {
		//								DatabaseManager.ModerationActions.Insert(new ModerationAction {
		//									ActionType = ModerationActionType.AccountPermanentBan,
		//									SourceCmid = publicProfile.Cmid,
		//									SourceName = publicProfile.Name,
		//									TargetCmid = profileToBan.Cmid,
		//									TargetName = profileToBan.Name,
		//									ActionDate = DateTime.UtcNow,
		//									ExpireTime = DateTime.MaxValue,
		//									Reason = reason,
		//								});

		//								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
		//							}
		//						} else {
		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
		//						}
		//					}
		//				} else {
		//					EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
		//				}

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] UnbanPlayer(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var targetCmid = Int32Proxy.Deserialize(bytes);

		//			DebugEndpoint(authToken, targetCmid);

		//			using (var outputStream = new MemoryStream()) {
		//				var steamMember = SteamMemberFromAuthToken(authToken);

		//				if (steamMember != null) {
		//					var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

		//					if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
		//						var bannedMember = DatabaseManager.ModerationActions.FindOne(_ => _.ActionType == ModerationActionType.AccountPermanentBan && _.TargetCmid == targetCmid);

		//						if (bannedMember == null || bannedMember.TargetCmid == publicProfile.Cmid) {
		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
		//							return outputStream.ToArray();
		//						} else {
		//							DatabaseManager.ModerationActions.DeleteMany(_ => _.ActionType == ModerationActionType.AccountPermanentBan && _.TargetCmid == targetCmid);

		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
		//						}
		//					}
		//				} else {
		//					EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
		//				}

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] MutePlayer(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var durationInMinutes = Int32Proxy.Deserialize(bytes);
		//			var mutedCmid = Int32Proxy.Deserialize(bytes);

		//			DebugEndpoint(authToken, durationInMinutes, mutedCmid);

		//			using (var outputStream = new MemoryStream()) {
		//				var steamMember = SteamMemberFromAuthToken(authToken);

		//				if (steamMember != null) {
		//					var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

		//					if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
		//						var profileToMute = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == mutedCmid);

		//						if (profileToMute == null || profileToMute.Cmid == publicProfile.Cmid) {
		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
		//							return outputStream.ToArray();
		//						}

		//						if (profileToMute.AccessLevel < publicProfile.AccessLevel) {
		//							if (DatabaseManager.ModerationActions.FindOne(_ => _.ActionType == ModerationActionType.ChatTemporaryBan && _.TargetCmid == profileToMute.Cmid && _.ExpireTime > DateTime.UtcNow) != null) {
		//								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
		//							} else {
		//								DatabaseManager.ModerationActions.Insert(new ModerationAction {
		//									ActionType = ModerationActionType.ChatTemporaryBan,
		//									SourceCmid = publicProfile.Cmid,
		//									SourceName = publicProfile.Name,
		//									TargetCmid = profileToMute.Cmid,
		//									TargetName = profileToMute.Name,
		//									ActionDate = DateTime.UtcNow,
		//									ExpireTime = DateTime.UtcNow.AddMinutes(durationInMinutes)
		//								});

		//								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
		//							}
		//						} else {
		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
		//						}
		//					}
		//				} else {
		//					EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
		//				}

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] UnmutePlayer(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);
		//			var mutedCmid = Int32Proxy.Deserialize(bytes);

		//			DebugEndpoint(authToken, mutedCmid);

		//			using (var outputStream = new MemoryStream()) {
		//				var steamMember = SteamMemberFromAuthToken(authToken);

		//				if (steamMember != null) {
		//					var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

		//					if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
		//						var mutedMember = DatabaseManager.ModerationActions.FindOne(_ => _.ActionType == ModerationActionType.ChatTemporaryBan && _.TargetCmid == mutedCmid && _.ExpireTime > DateTime.UtcNow);

		//						if (mutedMember == null || mutedMember.TargetCmid == publicProfile.Cmid) {
		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
		//						} else {
		//							DatabaseManager.ModerationActions.DeleteMany(_ => _.ActionType == ModerationActionType.ChatTemporaryBan && _.TargetCmid == mutedCmid);

		//							EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
		//						}
		//					}
		//				} else {
		//					EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
		//				}

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		//public byte[] GetNaughtyList(byte[] data) {
		//	try {
		//		using (var bytes = new MemoryStream(data)) {
		//			var authToken = StringProxy.Deserialize(bytes);

		//			DebugEndpoint(authToken);

		//			using (var outputStream = new MemoryStream()) {
		//				var steamMember = SteamMemberFromAuthToken(authToken);

		//				if (steamMember != null) {
		//					var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

		//					if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
		//						var moderationActions = DatabaseManager.ModerationActions.Find(_ => _.ExpireTime == null || _.ExpireTime >= DateTime.UtcNow);

		//						//ListProxy<CommActorInfo>.Serialize(outputStream, moderationActions.Select(_ => {
		//						//	return new CommActorInfo {

		//						//	};
		//						//}), CommActorInfoProxy.Serialize);

		//						ListProxy<CommActorInfo>.Serialize(outputStream, new List<CommActorInfo> {
		//							new CommActorInfo {
		//								AccessLevel = MemberAccessLevel.Default,
		//								Channel = ChannelType.Steam,
		//								ClanTag = "TST",
		//								Cmid = 1337,
		//								ModerationFlag = (byte)(ModerationFlag.Muted | ModerationFlag.Ghosted | ModerationFlag.Banned | ModerationFlag.Speed | ModerationFlag.Spamming | ModerationFlag.CrudeLanguage),
		//								PlayerName = "Test user"
		//							}
		//						}, CommActorInfoProxy.Serialize);
		//					}
		//				}

		//				return outputStream.ToArray();
		//			}
		//		}
		//	} catch (Exception e) {
		//		HandleEndpointError(e);
		//	}

		//	return null;
		//}

		public byte[] SetModerationFlag(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var moderationFlag = EnumProxy<ModerationFlag>.Deserialize(bytes);
					var expireTime = DateTimeProxy.Deserialize(bytes);
					var reason = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken, targetCmid, moderationFlag, expireTime, reason);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

							if (publicProfile == null || publicProfile.AccessLevel < MemberAccessLevel.Moderator) {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
							} else {
								var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
								} else if (targetProfile.AccessLevel < publicProfile.AccessLevel) {
									if (DatabaseManager.ModerationActions.FindOne(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag) is var moderationAction && moderationAction != null) {
										moderationAction.ActionDate = DateTime.UtcNow;
										moderationAction.ExpireTime = expireTime;
										moderationAction.SourceCmid = publicProfile.Cmid;
										moderationAction.SourceName = publicProfile.Name;

										DatabaseManager.ModerationActions.DeleteMany(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag);
										DatabaseManager.ModerationActions.Insert(moderationAction);
									} else {
										DatabaseManager.ModerationActions.Insert(new ModerationAction {
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

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] UnsetModerationFlag(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var moderationFlag = EnumProxy<ModerationFlag>.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

							if (publicProfile == null || publicProfile.AccessLevel < MemberAccessLevel.Moderator) {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
							} else {
								var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
								} else if (targetProfile.AccessLevel < publicProfile.AccessLevel) {
									if (DatabaseManager.ModerationActions.FindOne(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag) is var moderationAction && moderationAction != null) {
										//DatabaseManager.ModerationActions.DeleteMany(_ => _.TargetCmid == targetProfile.Cmid && _.ModerationFlag == moderationFlag);
										moderationAction.ExpireTime = DateTime.MinValue;

										DatabaseManager.ModerationActions.DeleteMany(_ => _.TargetCmid == targetCmid && _.ModerationFlag == moderationFlag);
										DatabaseManager.ModerationActions.Insert(moderationAction);

										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
									} else {
										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
									}
								} else {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
								}
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

		public byte[] ClearModerationFlags(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

							if (publicProfile == null || publicProfile.AccessLevel < MemberAccessLevel.Moderator) {
								EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);
							} else {
								var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (targetProfile == null || targetProfile.Cmid == publicProfile.Cmid) {
									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidCmid);
								} else if (targetProfile.AccessLevel < publicProfile.AccessLevel) {
									if (DatabaseManager.ModerationActions.Find(_ => _.TargetCmid == targetCmid) is var moderationActions) {
										//DatabaseManager.ModerationActions.DeleteMany(_ => _.TargetCmid == targetProfile.Cmid);
										foreach (var action in moderationActions) {
											action.ExpireTime = DateTime.MinValue;

											DatabaseManager.ModerationActions.DeleteMany(_ => _.TargetCmid == targetCmid && _.ModerationFlag == action.ModerationFlag);
											DatabaseManager.ModerationActions.Insert(action);
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

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		public byte[] GetNaughtyList(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = (steamMember != null) ? DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid) : null;

							if (publicProfile == null || publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
								var naughtyUsers = new List<CommActorInfo>();
								var moderationActions = DatabaseManager.ModerationActions.Find(_ => _.ExpireTime == null || _.ExpireTime > DateTime.UtcNow);

								foreach (var action in moderationActions) {
									if (naughtyUsers.Find(_ => _.Cmid.Equals(action.TargetCmid)) is var user && user != null) {
										user.ModerationFlag |= (byte)action.ModerationFlag;
									} else {
										var profile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == action.TargetCmid);
										var clan = DatabaseManager.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid.Equals(action.TargetCmid)) != null);

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

								foreach (var test in naughtyUsers) {
									Console.WriteLine($"{test.Cmid}: {test.ModerationFlag}");
								}

								ListProxy<CommActorInfo>.Serialize(outputStream, naughtyUsers, CommActorInfoProxy.Serialize);
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
