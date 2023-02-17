using LiteDB;
using log4net;
using Paradise.DataCenter.Common.Entities;
using System;
using System.IO;

namespace Paradise.WebServices {
	public enum DatabaseOperationResult {
		Unknown,
		OpenOk,
		CloseOk,
		NotOpened,
		AlreadyOpened,
		GenericError
	}

	public static class DatabaseManager {
		private static readonly ILog Log = LogManager.GetLogger(nameof(DatabaseManager));

		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		private static LiteDatabase databaseInstance;

		public static bool IsOpen => databaseInstance != null;

		// Authentication
		public static ILiteCollection<SteamMember> SteamMembers { get; private set; }

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

		public static EventHandler<EventArgs> DatabaseOpened;
		public static EventHandler<EventArgs> DatabaseClosed;

		static DatabaseManager() {
			BsonMapper.Global.Entity<PublicProfileView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<LoadoutView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<ContactRequestView>().Id(_ => _.RequestId);
			BsonMapper.Global.Entity<PrivateMessageView>().Id(_ => _.PrivateMessageId);
			//BsonMapper.Global.Entity<ClanView>().Id(_ => _.GroupId);
		}

		public static DatabaseOperationResult DisposeDatabase() {
			if (databaseInstance == null) {
				Log.Error($"Failed to save database tables: No connection to database!");
				return DatabaseOperationResult.NotOpened;
			}

			Log.Info($"Saving database tables... ");

			try {
				databaseInstance.Dispose();

				databaseInstance = null;

				SteamMembers = null;

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
			} catch (Exception e) {
				Log.Error($"Failed to save database tables: {e.Message}");
				Log.Debug(e);
				return DatabaseOperationResult.GenericError;
			}

			Log.Info($"Finished saving database tables.");
			DatabaseClosed?.Invoke(null, new EventArgs());

			return DatabaseOperationResult.CloseOk;
		}

		public static DatabaseOperationResult OpenDatabase() {
			if (databaseInstance != null) {
				Log.Error("Failed to connect to database: A database connection is already open!");
				return DatabaseOperationResult.AlreadyOpened;
			}

			Log.Info($"Connecting to database... ");

			try {
				databaseInstance = new LiteDatabase(Path.Combine(CurrentDirectory, "ServiceData", "Paradise.litedb"));

				SteamMembers = databaseInstance.GetCollection<SteamMember>("SteamMembers");
				SteamMembers.EnsureIndex("SteamId");

				ClanMembers = databaseInstance.GetCollection<ClanMemberView>("ClanMembers");

				Clans = databaseInstance.GetCollection<ClanView>("Clans");
				Clans.EnsureIndex("GroupId");

				GroupInvitations = databaseInstance.GetCollection<GroupInvitationView>("GroupInvitations");

				ModerationActions = databaseInstance.GetCollection<ModerationAction>("ModerationActions");

				MemberReports = databaseInstance.GetCollection<MemberReportView>("MemberReports");

				PrivateMessages = databaseInstance.GetCollection<PrivateMessageView>("PrivateMessages");
				PrivateMessages.EnsureIndex("PrivateMessageId");

				ContactRequests = databaseInstance.GetCollection<ContactRequestView>("ContactRequests");
				ContactRequests.EnsureIndex("RequestId");

				MemberWallets = databaseInstance.GetCollection<MemberWalletView>("MemberWallets");
				MemberWallets.EnsureIndex("Cmid");

				PlayerInventoryItems = databaseInstance.GetCollection<ItemInventoryView>("PlayerInventoryItems");

				PlayerLoadouts = databaseInstance.GetCollection<LoadoutView>("PlayerLoadouts");
				PlayerLoadouts.EnsureIndex("Cmid");

				PlayerStatistics = databaseInstance.GetCollection<PlayerStatisticsView>("PlayerStatistics");
				PlayerStatistics.EnsureIndex("Cmid");

				PublicProfiles = databaseInstance.GetCollection<PublicProfileView>("PublicProfiles");
				PublicProfiles.EnsureIndex("Cmid");

				ItemTransactions = databaseInstance.GetCollection<ItemTransactionView>("ItemTransactions");

				CurrencyDeposits = databaseInstance.GetCollection<CurrencyDepositView>("CurrencyDeposits");

				PointDeposits = databaseInstance.GetCollection<PointDepositView>("PointDeposits");
			} catch (Exception e) {
				Log.Error($"Failed to connect to database: {e.Message}");
				Log.Debug(e);
				return DatabaseOperationResult.GenericError;
			}

			Log.Info($"Database opened.");
			DatabaseOpened?.Invoke(null, new EventArgs());

			return DatabaseOperationResult.OpenOk;
		}

		public static DatabaseOperationResult ReloadDatabase() {
			DatabaseOperationResult result = default(DatabaseOperationResult);

			if (databaseInstance != null) {
				result = DisposeDatabase();
			}

			if (result != DatabaseOperationResult.CloseOk) return result;

			if (databaseInstance == null) {
				result = OpenDatabase();
			}

			if (result == DatabaseOperationResult.OpenOk) {
				Log.Info($"Finished reloading database tables.");
			}

			return result;
		}
	}
}
