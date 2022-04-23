using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Contracts;
using System;
using System.IO;
using System.ServiceModel;

namespace Paradise.WebServices.Services {
	public class ModerationWebService : WebServiceBase, IModerationWebServiceContract {
		public override string ServiceName => "ModerationWebService";
		public override string ServiceVersion => "2.0";
		protected override Type ServiceInterface => typeof(IModerationWebServiceContract);

		public ModerationWebService(BasicHttpBinding binding, string serviceBaseUrl, string webServicePrefix, string webServiceSuffix) : base(binding, serviceBaseUrl, webServicePrefix, webServiceSuffix) { }
		public ModerationWebService(BasicHttpBinding binding, ParadiseSettings settings, IServiceCallback serviceCallback) : base(binding, settings, serviceCallback) { }

		protected override void Setup() { }

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
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
								var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (targetProfile != null &&
									targetProfile.Cmid != publicProfile.Cmid &&
									targetProfile.AccessLevel < publicProfile.AccessLevel &&
									accessLevel < publicProfile.AccessLevel &&
									accessLevel > targetProfile.AccessLevel) {
									targetProfile.AccessLevel = accessLevel;

									DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
									DatabaseManager.PublicProfiles.Insert(targetProfile);

									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);

									return outputStream.ToArray();
								}
							}
						}

						EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.InvalidData);

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
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
								var targetProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (targetProfile != null &&
									targetProfile.Cmid != publicProfile.Cmid &&
									targetProfile.AccessLevel > MemberAccessLevel.Default &&
									targetProfile.AccessLevel < publicProfile.AccessLevel) {
									targetProfile.AccessLevel = MemberAccessLevel.Default;

									DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
									DatabaseManager.PublicProfiles.Insert(targetProfile);

									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);

									return outputStream.ToArray();
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
		/// Bans a user by Cmid permanently
		/// </summary>
		public byte[] BanPermanently(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var reason = StringProxy.Deserialize(bytes);

					DebugEndpoint(authToken, targetCmid, reason);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
								var profileToBan = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == targetCmid);

								if (profileToBan != null && profileToBan.Cmid != publicProfile.Cmid && profileToBan.AccessLevel < publicProfile.AccessLevel) {
									DatabaseManager.BannedMembers.DeleteMany(_ => _.TargetCmid == profileToBan.Cmid);
									DatabaseManager.BannedMembers.Insert(new BannedMember {
										IsBanned = true,
										BanningDate = DateTime.UtcNow,
										SourceCmid = publicProfile.Cmid,
										SourceName = publicProfile.Name,
										TargetCmid = profileToBan.Cmid,
										TargetName = profileToBan.Name,
										Reason = reason
									});

									EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
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

		public byte[] UnbanPlayer(byte[] data) {
			try {
				using (var bytes = new MemoryStream(data)) {
					var authToken = StringProxy.Deserialize(bytes);
					var targetCmid = Int32Proxy.Deserialize(bytes);

					DebugEndpoint(authToken, targetCmid);

					using (var outputStream = new MemoryStream()) {
						var steamMember = SteamMemberFromAuthToken(authToken);

						if (steamMember != null) {
							var publicProfile = DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == steamMember.Cmid);

							if (publicProfile != null && publicProfile.AccessLevel >= MemberAccessLevel.Moderator) {
								var bannedMember = DatabaseManager.BannedMembers.FindOne(_ => _.TargetCmid == targetCmid);

								if (bannedMember != null && bannedMember.TargetCmid != publicProfile.Cmid) {
									bannedMember.IsBanned = false;
									bannedMember.IsHwidBanned = false;
									bannedMember.IsIpBanned = false;
									bannedMember.BannedUntil = DateTime.UtcNow;

									DatabaseManager.BannedMembers.DeleteMany(_ => _.TargetCmid == bannedMember.TargetCmid);
									DatabaseManager.BannedMembers.Insert(bannedMember);

										EnumProxy<MemberOperationResult>.Serialize(outputStream, MemberOperationResult.Ok);
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
