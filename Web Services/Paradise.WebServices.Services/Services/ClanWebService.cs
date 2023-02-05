using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public enum ClanCreationResultCode {
		Success,
		InvalidClanName,
		ClanCollision,
		ClanNameTaken,
		InvalidClanTag,
		InvalidClanMotto = 8,
		ClanTagTaken = 10,
		RequirementPlayerLevel = 100,
		RequirementPlayerFriends,
		RequirementClanLicense
	}

	public enum ClanActionResultCode {
		Success,
		Error
	}

	public class ClanWebService : BaseWebService, IClanWebServiceContract {
		public override string ServiceName => "ClanWebService";
		public override string ServiceVersion => ApiVersion.Current;
		protected override Type ServiceInterface => typeof(IClanWebServiceContract);

		public ClanWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public ClanWebService(BasicHttpBinding binding, ParadiseServerSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }
		protected override void Teardown() { }

		/// <summary>
		/// Accepts a clan invitation on the invitee side
		/// </summary>
		public byte[] AcceptClanInvitation(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clanInvitationId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(clanInvitationId, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (publicProfile != null) {
								var groupInvitation = DatabaseManager.GroupInvitations.FindOne(_ => _.GroupInvitationId == clanInvitationId);

								if (groupInvitation != null) {
									var clan = DatabaseManager.Clans.FindOne(_ => _.GroupId == groupInvitation.GroupId);

									if (clan != null) {
										if (clan.Members.Find(_ => _.Cmid == steamMember.Cmid) == null) {
											clan.Members.Add(new ClanMemberView {
												Cmid = publicProfile.Cmid,
												Name = publicProfile.Name,
												Position = GroupPosition.Member,
												JoiningDate = DateTime.UtcNow,
												Lastlogin = publicProfile.LastLoginDate
											});

											DatabaseManager.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
											DatabaseManager.Clans.Insert(clan);

											DatabaseManager.GroupInvitations.DeleteMany(_ => _.GroupInvitationId == groupInvitation.GroupInvitationId);

											ClanRequestAcceptViewProxy.Serialize(outputStream, new ClanRequestAcceptView {
												ActionResult = (int)ClanActionResultCode.Success,
												ClanRequestId = clanInvitationId,
												ClanView = clan
											});

											return outputStream.ToArray();
										}
									}
								}
							}
						}

						ClanRequestAcceptViewProxy.Serialize(outputStream, new ClanRequestAcceptView {
							ActionResult = (int)ClanActionResultCode.Error,
							ClanRequestId = clanInvitationId
						});

						return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Cancels a pending clan invitation sent by a clan member
		/// </summary>
		public byte[] CancelInvitation(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var groupInvitationId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(groupInvitationId, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var groupInvitation = DatabaseManager.GroupInvitations.FindOne(_ => _.GroupInvitationId == groupInvitationId);

							if (groupInvitation != null) {
								DatabaseManager.GroupInvitations.DeleteMany(_ => _.GroupInvitationId == groupInvitationId);

								Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Success);
							} else {
								Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
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
		/// Checks if a user can actually own a clan. Appears to be unused by the game client.
		/// </summary>
		public byte[] CanOwnAClan(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						throw new NotImplementedException();

						//return outputStream.ToArray();
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Creates a new clan
		/// </summary>
		public byte[] CreateClan(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var createClanData = GroupCreationViewProxy.Deserialize(bytes);

					DebugEndpoint(createClanData);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(createClanData.AuthToken);

						if (steamMember != null) {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (publicProfile != null) {
								foreach (var clan in DatabaseManager.Clans.FindAll()) {
									if (clan.Members.Find(_ => _.Cmid == steamMember.Cmid) != null) {
										// "Clan Collision", "You are already member of another clan, please leave first before creating your own."
										ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
											ResultCode = (int)ClanCreationResultCode.ClanCollision
										});

										return outputStream.ToArray();
									}
								}

								var friendsList = DatabaseManager.ContactRequests.Find(_ => (_.InitiatorCmid == publicProfile.Cmid || _.ReceiverCmid == publicProfile.Cmid) && _.Status == ContactRequestStatus.Accepted);
								var playerStatistics = DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == steamMember.Cmid);
								var hasClanLicense = DatabaseManager.PlayerInventoryItems.FindOne(_ => _.Cmid == steamMember.Cmid && _.ItemId == (int)UberstrikeInventoryItem.ClanLicense) != null;

								var profanityFilter = new ProfanityFilter.ProfanityFilter();

								if (profanityFilter.ContainsProfanity(createClanData.Name)) {
									// "Invalid Clan Name", "The name '" + name + "' is not valid, please modify it."

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.InvalidClanName
									});
								} else if (DatabaseManager.Clans.FindOne(_ => _.Name == createClanData.Name) != null) {
									// "Clan Name", "The name '" + name + "' is already taken, try another one."

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.ClanNameTaken
									});
								} else if (profanityFilter.ContainsProfanity(createClanData.Tag)) {
									// "Invalid Clan Tag", "The tag '" + tag + "' is not valid, please modify it."

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.InvalidClanTag
									});
								} else if (profanityFilter.ContainsProfanity(createClanData.Motto)) {
									//"Invalid Clan Motto", "The motto '" + motto + "' is not valid, please modify it."

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.InvalidClanMotto
									});
								} else if (DatabaseManager.Clans.FindOne(_ => _.Tag == createClanData.Tag) != null) {
									// "Clan Tag", "The tag '" + tag + "' is already taken, try another one."

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.ClanTagTaken
									});
#if DEBUG
								} else if (XpPointsUtil.GetLevelForXp(playerStatistics.Xp) < 4 && false) {
									// "Sorry", "You don't fulfill the minimal requirements to create your own clan."

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.RequirementPlayerLevel
									});
								} else if (friendsList.Count() < 1 && false) {
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.RequirementPlayerFriends
									});
								} else if (!hasClanLicense && false) {
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.RequirementClanLicense
									});
#else
								} else if (XpPointsUtil.GetLevelForXp(playerStatistics.Xp) < 4) {
									// "Sorry", "You don't fulfill the minimal requirements to create your own clan."

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.RequirementPlayerLevel
									});
								} else if (friendsList.Count() < 1) {
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.RequirementPlayerFriends
									});
								} else if (!hasClanLicense) {
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.RequirementClanLicense
									});
#endif
								} else {
									var r = new Random((int)DateTime.UtcNow.Ticks);

									var clan = new ClanView {
										GroupId = r.Next(0, int.MaxValue),
										Name = createClanData.Name,
										Motto = createClanData.Motto,
										FoundingDate = DateTime.UtcNow,
										Type = GroupType.Clan,
										LastUpdated = DateTime.UtcNow,
										Tag = createClanData.Tag,
										MembersLimit = 12,
										ApplicationId = createClanData.ApplicationId,
										OwnerCmid = publicProfile.Cmid,
										OwnerName = publicProfile.Name
									};

									clan.Members.Add(new ClanMemberView {
										Cmid = publicProfile.Cmid,
										Name = publicProfile.Name,
										Position = GroupPosition.Leader,
										JoiningDate = DateTime.UtcNow,
										Lastlogin = publicProfile.LastLoginDate
									});

									DatabaseManager.Clans.Insert(clan);

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = (int)ClanCreationResultCode.Success,
										ClanView = clan
									});
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

		/// <summary>
		/// Declines a pending clan invitation on the invitee side
		/// </summary>
		public byte[] DeclineClanInvitation(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clanInvitationId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(clanInvitationId, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var groupInvitation = DatabaseManager.GroupInvitations.FindOne(_ => _.GroupInvitationId == clanInvitationId);

							if (groupInvitation != null) {
								DatabaseManager.GroupInvitations.DeleteMany(_ => _.GroupInvitationId == clanInvitationId);

								ClanRequestDeclineViewProxy.Serialize(outputStream, new ClanRequestDeclineView {
									ActionResult = (int)ClanActionResultCode.Success,
									ClanRequestId = clanInvitationId
								});
							} else {
								ClanRequestDeclineViewProxy.Serialize(outputStream, new ClanRequestDeclineView {
									ActionResult = (int)ClanActionResultCode.Error,
									ClanRequestId = clanInvitationId
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

		/// <summary>
		/// Completely removes a clan
		/// </summary>
		public byte[] DisbandGroup(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var groupId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(groupId, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var clan = DatabaseManager.Clans.FindOne(_ => _.GroupId == groupId);

							if (clan != null && clan.Members.Find(_ => _.Cmid == steamMember.Cmid && _.Position == GroupPosition.Leader) != null) {
								DatabaseManager.Clans.DeleteMany(_ => _.GroupId == groupId);
								DatabaseManager.GroupInvitations.DeleteMany(_ => _.GroupId == groupId);

								Int32Proxy.Serialize(outputStream, 0);
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
		/// Returns a list of all clan/group invitations sent to a player
		/// </summary>
		public byte[] GetAllGroupInvitations(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var groupInvitations = DatabaseManager.GroupInvitations.Find(_ => _.InviteeCmid == steamMember.Cmid).ToList();

							ListProxy<GroupInvitationView>.Serialize(outputStream, groupInvitations, GroupInvitationViewProxy.Serialize);
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
		/// Returns the clan ID of a player's current clan
		/// </summary>
		public byte[] GetMyClanId(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							if (DatabaseManager.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid.Equals(steamMember.Cmid)) != null) is ClanView clan) {
								Int32Proxy.Serialize(outputStream, clan.GroupId);
							} else {
								Int32Proxy.Serialize(outputStream, 0);
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
		/// Returns the data of a player's current clan
		/// </summary>
		public byte[] GetOwnClan(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var groupId = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, groupId);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							if (DatabaseManager.Clans.FindAll().ToList().FirstOrDefault(_ => _.Members.Find(__ => __.Cmid.Equals(steamMember.Cmid)) != null) is ClanView clan) {
								ClanViewProxy.Serialize(outputStream, clan);
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
		/// Gets a list of all pending invites sent by a clan
		/// </summary>
		public byte[] GetPendingGroupInvitations(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var groupId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(groupId, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var groupInvitations = DatabaseManager.GroupInvitations.Find(_ => _.GroupId == groupId && _.InviterCmid == steamMember.Cmid).ToList();

							ListProxy<GroupInvitationView>.Serialize(outputStream, groupInvitations, GroupInvitationViewProxy.Serialize);
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
		/// Invites a member to join a clan/group
		/// </summary>
		public byte[] InviteMemberToJoinAGroup(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var clanId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var inviteeCmid = Int32Proxy.Deserialize(bytes);
					var message = StringProxy.Deserialize(bytes);

					DebugEndpoint(clanId, authToken, inviteeCmid, message);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var inviteeProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == inviteeCmid);

							if (publicProfile != null && inviteeProfile != null) {
								var clan = DatabaseManager.Clans.FindOne(_ => _.GroupId == clanId);

								if (clan != null &&
									clan.Members.Find(_ => _.Cmid == inviteeCmid) == null &&
									DatabaseManager.GroupInvitations.FindOne(_ => _.GroupId == clanId && _.InviteeCmid == inviteeCmid) == null) {
									var r = new Random((int)DateTime.UtcNow.Ticks);

									DatabaseManager.GroupInvitations.Insert(new GroupInvitationView {
										InviterCmid = publicProfile.Cmid,
										InviterName = publicProfile.Name,
										GroupName = clan.Name,
										GroupTag = clan.Tag,
										GroupId = clan.GroupId,
										GroupInvitationId = r.Next(0, int.MaxValue),
										InviteeCmid = inviteeCmid,
										InviteeName = inviteeProfile.Name,
										Message = message
									});
								} else {
									Log.Error("no clan or existing group invitation");
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

		/// <summary>
		/// Removes a member from a clan
		/// </summary>
		public byte[] KickMemberFromClan(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var groupId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var cmidToKick = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(groupId, authToken, cmidToKick);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember == null) {
							Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
						} else {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var toKickProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmidToKick);


							if (publicProfile == null || toKickProfile == null) {
								Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
							} else {
								var clan = DatabaseManager.Clans.FindOne(_ => _.GroupId == groupId);

								if (clan == null) {
									Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
								} else {
									var clanMember = clan.Members.Find(_ => _.Cmid == publicProfile.Cmid);
									var memberToKick = clan.Members.Find(_ => _.Cmid == cmidToKick);

									if ((clanMember == null || memberToKick == null) ||
										(memberToKick.Position == GroupPosition.Officer && clanMember.Position != GroupPosition.Leader) ||
										(memberToKick.Position == GroupPosition.Member && !(clanMember.Position == GroupPosition.Officer || clanMember.Position == GroupPosition.Leader))) {
										Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
									} else {
										clan.Members.Remove(memberToKick);

										DatabaseManager.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
										DatabaseManager.Clans.Insert(clan);

										Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Success);
									}
								}
							}
						}
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Makes a user leave a specified clan
		/// </summary>
		public byte[] LeaveAClan(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var groupId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);

					DebugEndpoint(groupId, authToken);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember == null) {
							Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
						} else {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (publicProfile == null) {
								Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
							} else {
								var clan = DatabaseManager.Clans.FindOne(_ => _.GroupId == groupId);

								if (clan == null) {
									Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
								} else {
									var clanMember = clan.Members.Find(_ => _.Cmid == publicProfile.Cmid);

									if (clanMember == null || clanMember.Position == GroupPosition.Leader) {
										Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
									} else {
										clan.Members.Remove(clanMember);

										DatabaseManager.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
										DatabaseManager.Clans.Insert(clan);

										Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Success);
									}
								}
							}
						}
					}
				}
			} catch (Exception e) {
				HandleEndpointError(e);
			}

			return null;
		}

		/// <summary>
		/// Transfers ownership of a clan to a different clan member
		/// </summary>
		public byte[] TransferOwnership(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var groupId = Int32Proxy.Deserialize(bytes);
					var authToken = StringProxy.Deserialize(bytes);
					var newLeaderCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(groupId, authToken, newLeaderCmid);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember == null) {
							Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
						} else {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var newLeaderProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == newLeaderCmid);

							if (publicProfile == null || newLeaderProfile == null) {
								Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
							} else {
								var clan = DatabaseManager.Clans.FindOne(_ => _.GroupId == groupId);

								if (clan == null) {
									Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
								} else {
									var clanMember = clan.Members.Find(_ => _.Cmid == publicProfile.Cmid);
									var newLeader = clan.Members.Find(_ => _.Cmid == newLeaderCmid);

									if ((clanMember == null || newLeader == null) ||
										clanMember.Position != GroupPosition.Leader) {
										Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
									} else {
										var friendsList = DatabaseManager.ContactRequests.Find(_ => (_.InitiatorCmid == newLeaderProfile.Cmid || _.ReceiverCmid == publicProfile.Cmid) && _.Status == ContactRequestStatus.Accepted);
										var playerStatistics = DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == newLeaderCmid);
										var hasClanLicense = DatabaseManager.PlayerInventoryItems.FindOne(_ => _.Cmid == newLeaderCmid && _.ItemId == (int)UberstrikeInventoryItem.ClanLicense) != null;

										if (XpPointsUtil.GetLevelForXp(playerStatistics.Xp) < 4) {
											Int32Proxy.Serialize(outputStream, (int)ClanCreationResultCode.RequirementPlayerLevel);
										} else if (friendsList.Count() < 1) {
											Int32Proxy.Serialize(outputStream, (int)ClanCreationResultCode.RequirementPlayerFriends);
										} else if (!hasClanLicense) {
											Int32Proxy.Serialize(outputStream, (int)ClanCreationResultCode.RequirementClanLicense);
										} else {
											clan.OwnerCmid = newLeader.Cmid;
											clan.OwnerName = newLeader.Name;

											clanMember.Position = newLeader.Position;
											newLeader.Position = GroupPosition.Leader;

											DatabaseManager.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
											DatabaseManager.Clans.Insert(clan);

											Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Success);
										}
									}
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

		/// <summary>
		/// Updates the position of a clan member
		/// </summary>
		public byte[] UpdateMemberPosition(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var updateMemberPositionData = MemberPositionUpdateViewProxy.Deserialize(bytes);

					DebugEndpoint(updateMemberPositionData);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(updateMemberPositionData.AuthToken);

						if (steamMember == null) {
							Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
						} else {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);
							var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == updateMemberPositionData.MemberCmid);

							if (publicProfile == null && targetProfile == null) {
								Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
							} else {
								var clan = DatabaseManager.Clans.FindOne(_ => _.GroupId == updateMemberPositionData.GroupId);

								if (clan == null) {
									Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
									return outputStream.ToArray();
								} else {
									var clanMember = clan.Members.Find(_ => _.Cmid == publicProfile.Cmid);
									var targetClanMember = clan.Members.Find(_ => _.Cmid == updateMemberPositionData.MemberCmid);

									if ((clanMember == null || targetClanMember == null) ||
										(targetClanMember.Position == GroupPosition.Officer && clanMember.Position != GroupPosition.Leader) ||
										(targetClanMember.Position == GroupPosition.Member && !(clanMember.Position == GroupPosition.Officer || clanMember.Position == GroupPosition.Leader))) {
										Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Error);
									} else {
										targetClanMember.Position = updateMemberPositionData.Position;

										DatabaseManager.Clans.DeleteMany(_ => _.GroupId == clan.GroupId);
										DatabaseManager.Clans.Insert(clan);

										Int32Proxy.Serialize(outputStream, (int)ClanActionResultCode.Success);
									}
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
	}
}
