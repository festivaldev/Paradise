using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Paradise.WebServices.Services {
	public class ClanWebService : WebServiceBase, IClanWebServiceContract {
		public override string ServiceName => "ClanWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IClanWebServiceContract);

		public ClanWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public ClanWebService(WebServiceConfiguration serviceConfig, IServiceCallback serviceCallback) : base(serviceConfig, serviceCallback) { }

		protected override void Setup() { }

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
												JoiningDate = DateTime.Now,
												Lastlogin = publicProfile.LastLoginDate
											});

											DatabaseManager.Clans.Update(clan);

											ClanRequestAcceptViewProxy.Serialize(outputStream, new ClanRequestAcceptView { 
												ActionResult = 0,
												ClanRequestId = clanInvitationId,
												ClanView = clan
											});
										}
									}
								}
							}
						}

						ClanRequestAcceptViewProxy.Serialize(outputStream, new ClanRequestAcceptView {
							ActionResult = 1,
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

								Int32Proxy.Serialize(outputStream, 0);
							} else {
								Int32Proxy.Serialize(outputStream, 1);
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

						return outputStream.ToArray();
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
											ResultCode = 2
										});

										return outputStream.ToArray();
									}
								}

								if (DatabaseManager.Clans.FindOne(_ => _.Name == createClanData.Name) != null) {
									// "Clan Name", "The name '" + name + "' is already taken, try another one."
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = 3
									});
								} else if (DatabaseManager.Clans.FindOne(_ => _.Tag == createClanData.Tag) != null) {
									// "Clan Tag", "The tag '" + tag + "' is already taken, try another one."
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = 10
									});
								} else if (/*player.level < 4*/ false) {
									// "Sorry", "You don't fulfill the minimal requirements to create your own clan."
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = 100
									});
								} else if (/* player.friends.count < 1 */ false) {
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = 101
									});
								} else if (/* player.inventory.contains(1234) */ false) {
									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = 102
									});
								} else {
									var r = new Random((int)DateTime.Now.Ticks);

									var clan = new ClanView {
										GroupId = r.Next(0, int.MaxValue),
										Name = createClanData.Name,
										Motto = createClanData.Motto,
										FoundingDate = DateTime.Now,
										Type = GroupType.Clan,
										LastUpdated = DateTime.Now,
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
										JoiningDate = DateTime.Now,
										Lastlogin = publicProfile.LastLoginDate
									});

									DatabaseManager.Clans.Insert(clan);

									ClanCreationReturnViewProxy.Serialize(outputStream, new ClanCreationReturnView {
										ResultCode = 0,
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
									ActionResult = 0,
									ClanRequestId = clanInvitationId
								});
							} else {
								ClanRequestDeclineViewProxy.Serialize(outputStream, new ClanRequestDeclineView {
									ActionResult = 1,
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
								Console.WriteLine($"clan id: {clanId}, clan: {clan}");

								if (clan != null && DatabaseManager.GroupInvitations.FindOne(_ => _.GroupId == clanId && _.InviteeCmid == inviteeCmid) == null) {
									var r = new Random((int)DateTime.Now.Ticks);

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
						throw new NotImplementedException();

						return outputStream.ToArray();
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
						throw new NotImplementedException();

						return outputStream.ToArray();
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
						throw new NotImplementedException();

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
						throw new NotImplementedException();

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
