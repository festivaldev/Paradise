using Cmune.DataCenter.Common.Entities;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.Services {
	public static class DatabaseClient {
		#region Database Collections
		// Authentication
		public static ILiteCollection<SteamMember> SteamMembers { get; private set; }
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
			BsonMapper.Global.Entity<ClanMemberView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<ClanView>().Id(_ => _.GroupId);
			BsonMapper.Global.Entity<ContactRequestView>().Id(_ => _.RequestId);

			BsonMapper.Global.Entity<MemberWalletView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<LoadoutView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<PlayerStatisticsView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<PublicProfileView>().Id(_ => _.Cmid);

			BsonMapper.Global.Entity<ItemTransactionView>().Id(_ => _.WithdrawalId);
			BsonMapper.Global.Entity<CurrencyDepositView>().Id(_ => _.CreditsDepositId);
			BsonMapper.Global.Entity<PointDepositView>().Id(_ => _.PointDepositId);
		}

		public static void LoadCollections() {
			SteamMembers = DatabaseManager.Database.GetCollection<SteamMember>("SteamMembers");
			GameSessions = DatabaseManager.Database.GetCollection<GameSession>("GameSessions");

			ClanMembers = DatabaseManager.Database.GetCollection<ClanMemberView>("ClanMembers");
			Clans = DatabaseManager.Database.GetCollection<ClanView>("Clans");
			GroupInvitations = DatabaseManager.Database.GetCollection<GroupInvitationView>("GroupInvitations");

			ModerationActions = DatabaseManager.Database.GetCollection<ModerationAction>("ModerationActions");
			MemberReports = DatabaseManager.Database.GetCollection<MemberReportView>("MemberReports");

			PrivateMessages = DatabaseManager.Database.GetCollection<PrivateMessageView>("PrivateMessages");

			ContactRequests = DatabaseManager.Database.GetCollection<ContactRequestView>("ContactRequests");

			MemberWallets = DatabaseManager.Database.GetCollection<MemberWalletView>("MemberWallets");
			PlayerInventoryItems = DatabaseManager.Database.GetCollection<ItemInventoryView>("PlayerInventoryItems");
			PlayerLoadouts = DatabaseManager.Database.GetCollection<LoadoutView>("PlayerLoadouts");
			PlayerStatistics = DatabaseManager.Database.GetCollection<PlayerStatisticsView>("PlayerStatistics");
			PublicProfiles = DatabaseManager.Database.GetCollection<PublicProfileView>("PublicProfiles");

			ItemTransactions = DatabaseManager.Database.GetCollection<ItemTransactionView>("ItemTransactions");
			CurrencyDeposits = DatabaseManager.Database.GetCollection<CurrencyDepositView>("CurrencyDeposits");
			PointDeposits = DatabaseManager.Database.GetCollection<PointDepositView>("PointDeposits");
		}

		public static void UnloadCollections() {
			SteamMembers = null;
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
