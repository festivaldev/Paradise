using Cmune.DataCenter.Common.Entities;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.LegacyServices {
	public static class DatabaseClient {
		#region Database Collections
		// Authentication
		public static ILiteCollection<UserAccount> UserAccounts { get; private set; }
		public static ILiteCollection<GameSession> GameSessions { get; private set; }

		// Clans
		public static ILiteCollection<ClanMemberView> ClanMembers { get; private set; }
		public static ILiteCollection<ClanView> Clans { get; private set; }
		public static ILiteCollection<GroupInvitationView> GroupInvitations { get; private set; }

		// Moderation
		public static ILiteCollection<ModerationAction> ModerationActions { get; private set; }
		public static ILiteCollection<MemberReportView> MemberReports { get; private set; }

		// Private Messages
		public static ILiteCollection<PrivateMessageView> PrivateMessages { get; private set; }

		// Relationships
		public static ILiteCollection<ContactRequestView> ContactRequests { get; private set; }

		// User
		public static ILiteCollection<MemberWalletView> MemberWallets { get; private set; }
		public static ILiteCollection<ItemInventoryView> PlayerInventoryItems { get; private set; }
		public static ILiteCollection<LoadoutView> PlayerLoadouts { get; private set; }
		public static ILiteCollection<PlayerStatisticsView> PlayerStatistics { get; private set; }
		public static ILiteCollection<PublicProfileView> PublicProfiles { get; private set; }

		public static ILiteCollection<ItemTransactionView> ItemTransactions { get; private set; }
		public static ILiteCollection<CurrencyDepositView> CurrencyDeposits { get; private set; }
		public static ILiteCollection<PointDepositView> PointDeposits { get; private set; }
		#endregion

		static DatabaseClient() {
			BsonMapper.Global.Entity<PublicProfileView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<LoadoutView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<ContactRequestView>().Id(_ => _.RequestId);
			BsonMapper.Global.Entity<PrivateMessageView>().Id(_ => _.PrivateMessageId);
		}

		public static void LoadCollections() {
			UserAccounts = DatabaseManager.Database.GetCollection<UserAccount>("UserAccounts_Legacy");
			UserAccounts.EnsureIndex("Cmid");

			GameSessions = DatabaseManager.Database.GetCollection<GameSession>("GameSessions_Legacy");
			GameSessions.EnsureIndex("SessionId");

			ClanMembers = DatabaseManager.Database.GetCollection<ClanMemberView>("ClanMembers_Legacy");

			Clans = DatabaseManager.Database.GetCollection<ClanView>("Clans");
			Clans.EnsureIndex("GroupId");

			GroupInvitations = DatabaseManager.Database.GetCollection<GroupInvitationView>("GroupInvitations_Legacy");

			ModerationActions = DatabaseManager.Database.GetCollection<ModerationAction>("ModerationActions_Legacy");

			MemberReports = DatabaseManager.Database.GetCollection<MemberReportView>("MemberReports_Legacy");

			PrivateMessages = DatabaseManager.Database.GetCollection<PrivateMessageView>("PrivateMessages_Legacy");
			PrivateMessages.EnsureIndex("PrivateMessageId");

			ContactRequests = DatabaseManager.Database.GetCollection<ContactRequestView>("ContactRequests_Legacy");
			ContactRequests.EnsureIndex("RequestId");

			MemberWallets = DatabaseManager.Database.GetCollection<MemberWalletView>("MemberWallets_Legacy");
			MemberWallets.EnsureIndex("Cmid");

			PlayerInventoryItems = DatabaseManager.Database.GetCollection<ItemInventoryView>("PlayerInventoryItems_Legacy");

			PlayerLoadouts = DatabaseManager.Database.GetCollection<LoadoutView>("PlayerLoadouts_Legacy");
			PlayerLoadouts.EnsureIndex("Cmid");

			PlayerStatistics = DatabaseManager.Database.GetCollection<PlayerStatisticsView>("PlayerStatistics_Legacy");
			PlayerStatistics.EnsureIndex("Cmid");

			PublicProfiles = DatabaseManager.Database.GetCollection<PublicProfileView>("PublicProfiles_Legacy");
			PublicProfiles.EnsureIndex("Cmid");

			ItemTransactions = DatabaseManager.Database.GetCollection<ItemTransactionView>("ItemTransactions_Legacy");

			CurrencyDeposits = DatabaseManager.Database.GetCollection<CurrencyDepositView>("CurrencyDeposits_Legacy");

			PointDeposits = DatabaseManager.Database.GetCollection<PointDepositView>("PointDeposits_Legacy");
		}

		public static void UnloadCollections() {
			UserAccounts = null;
			GameSessions = null;

			ClanMembers = null;
			Clans = null;
			GroupInvitations = null;

			ModerationActions = null;
			MemberReports = null;

			PrivateMessages = null;

			ContactRequests = null;

			MemberWallets = null;
			PlayerInventoryItems = null;
			PlayerLoadouts = null;
			PlayerStatistics = null;
			PublicProfiles = null;

			ItemTransactions = null;
			CurrencyDeposits = null;
			PointDeposits = null;
		}

		public static PublicProfileView GetProfile(string search) {
			var playerProfiles = DatabaseClient.PublicProfiles.FindAll().ToList().OrderBy(_ => _.Name);
			var players = playerProfiles.Where(_ => _.Name.ToLowerInvariant().Contains(search.ToLowerInvariant()) || _.Cmid.ToString().Contains(search));

			return players.First();
		}

		public static List<PublicProfileView> GetProfiles(string search) {
			var playerProfiles = DatabaseClient.PublicProfiles.FindAll().ToList().OrderBy(_ => _.Name);
			var players = playerProfiles.Where(_ => _.Name.ToLowerInvariant().Contains(search.ToLowerInvariant()) || _.Cmid.ToString().Contains(search));

			return players.ToList();
		}
	}
}
