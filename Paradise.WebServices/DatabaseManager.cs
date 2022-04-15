using LiteDB;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace Paradise.WebServices {
	public class SteamMember {
		public string SteamId { get; set; }
		public int Cmid { get; set; }
		public string AuthToken { get; set; }
		public string MachineId { get; set; }
	}

	public class BannedMember {
		public int SourceCmid { get; set; }
		public string SourceName { get; set; }
		public int TargetCmid { get; set; }
		public string TargetName { get; set; }
		public bool IsBanned { get; set; }
		public bool IsIpBanned { get; set; }
		public bool IsHwidBanned { get; set; }
		public string Reason { get; set; }
		public DateTime? BannedUntil { get; set; }
		public DateTime BanningDate { get; set; }
		public long IpAddress { get; set; }
		public string Hwid { get; set; }
	}

	public static class DatabaseManager {
		private static LiteDatabase databaseInstance;

		// Authentication
		public static ILiteCollection<SteamMember> SteamMembers { get; private set; }

		// Clans
		public static ILiteCollection<ClanMemberView> ClanMembers { get; private set; }
		public static ILiteCollection<ClanView> Clans { get; private set; }
		public static ILiteCollection<GroupInvitationView> GroupInvitations { get; private set; }

		// Moderation
		public static ILiteCollection<BannedMember> BannedMembers { get; private set; }

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

		static DatabaseManager() {
			BsonMapper.Global.Entity<PublicProfileView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<LoadoutView>().Id(_ => _.Cmid);
			BsonMapper.Global.Entity<ContactRequestView>().Id(_ => _.RequestId);
			BsonMapper.Global.Entity<PrivateMessageView>().Id(_ => _.PrivateMessageId);
			//BsonMapper.Global.Entity<ClanView>().Id(_ => _.GroupId);
		}

		public static void DisposeDatabase() {
			if (databaseInstance == null) {
				Log.Error($"Failed to save database tables: No connection to database!");
				return;
			}

			Log.Info($"Saving database tables... ");

			try {
				databaseInstance.Dispose();

				databaseInstance = null;

				SteamMembers = null;
				ClanMembers = null;
				Clans = null;
				GroupInvitations = null;
				BannedMembers = null;
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
				return;
			}

			Log.Success($"Finished saving database tables.");
		}

		public static void OpenDatabase() {
			if (databaseInstance != null) {
				Log.Error("Failed to connect to database: A database connection is already open!");
				return;
			}

			Log.Info($"Connecting to database... ");

			try {
				databaseInstance = new LiteDatabase(Path.Combine(Directory.GetCurrentDirectory(), "Data", "Paradise.db"));

				SteamMembers = databaseInstance.GetCollection<SteamMember>("SteamMembers");
				SteamMembers.EnsureIndex("SteamId");

				ClanMembers = databaseInstance.GetCollection<ClanMemberView>("ClanMembers");

				Clans = databaseInstance.GetCollection<ClanView>("Clans");
				Clans.EnsureIndex("GroupId");

				GroupInvitations = databaseInstance.GetCollection<GroupInvitationView>("GroupInvitations");

				BannedMembers = databaseInstance.GetCollection<BannedMember>("BannedMembers");
				BannedMembers.EnsureIndex("Cmid");

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
				return;
			}

			Log.Success($"Database opened.");
		}

		public static void ReloadDatabase() {
			if (databaseInstance != null) {
				DisposeDatabase();
			}

			if (databaseInstance == null) {
				OpenDatabase();
			}

			Log.Success($"Finished reloading database tables.");
		}
	}
}
