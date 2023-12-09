using Cmune.DataCenter.Common.Entities;
using LiteDB;
using Paradise.WebServices.Services;

namespace Paradise.WebServices.Discord {
	public static class DatabaseClient {
		#region Database Collections
		public static ILiteCollection<DiscordUser> DiscordUsers { get; private set; }
		public static ILiteCollection<SteamMember> SteamMembers { get; private set; }
		public static ILiteCollection<PublicProfileView> PublicProfiles { get; private set; }
		#endregion

		static DatabaseClient() {
			BsonMapper.Global.Entity<PublicProfileView>().Id(_ => _.Cmid);
		}

		public static void LoadCollections() {
			DiscordUsers = DatabaseManager.Database.GetCollection<DiscordUser>("DiscordUsers");
			DiscordUsers.EnsureIndex("DiscordUserId");

			SteamMembers = DatabaseManager.Database.GetCollection<SteamMember>("SteamMembers");
			SteamMembers.EnsureIndex("SteamId");

			PublicProfiles = DatabaseManager.Database.GetCollection<PublicProfileView>("PublicProfiles");
			PublicProfiles.EnsureIndex("Cmid");
		}

		public static void UnloadCollections() {
			DiscordUsers = null;
			SteamMembers = null;
			PublicProfiles = null;
		}
	}
}
