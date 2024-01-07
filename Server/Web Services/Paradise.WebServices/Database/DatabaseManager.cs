using LiteDB;
using log4net;
using System;
using System.IO;

namespace Paradise.WebServices {
	public class DatabaseManager {
		public enum DatabaseOperationResult {
			Unknown,
			OpenOk,
			CloseOk,
			NotOpened,
			AlreadyOpened,
			GenericError
		}

		public class DatabaseEventArgs : EventArgs {
			public LiteDatabase Database;
			public DatabaseOperationResult Result;
			public Exception Exception;
		}

		private static readonly ILog Log = LogManager.GetLogger(nameof(DatabaseManager));
		private static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		public static LiteDatabase Database { get; private set; }

		public static bool IsOpen => Database != null;

		public static EventHandler<EventArgs> DatabaseOpened;
		public static EventHandler<EventArgs> DatabaseClosed;
		public static EventHandler<ErrorEventArgs> DatabaseError;

		public static DatabaseOperationResult DisposeDatabase() {
			if (Database == null) {
				Log.Error($"Failed to save database tables: No connection to database!");
				return DatabaseOperationResult.NotOpened;
			}

			Log.Info($"Saving database tables... ");

			try {
				Database.Dispose();
				Database = null;
			} catch (Exception e) {
				Log.Error($"Failed to save database tables: {e.Message}");
				Log.Debug(e);

				DatabaseError?.Invoke(null, new ErrorEventArgs(e));

				return DatabaseOperationResult.GenericError;
			}

			Log.Info($"Finished saving database tables.");
			DatabaseClosed?.Invoke(null, new EventArgs());

			return DatabaseOperationResult.CloseOk;
		}

		public static DatabaseOperationResult OpenDatabase() {
			if (Database != null) {
				Log.Error("Failed to connect to database: A database connection is already open!");
				return DatabaseOperationResult.AlreadyOpened;
			}

			Log.Info($"Connecting to database... ");

			try {
				string dbPath;
				if (Path.IsPathRooted(ParadiseServerSettings.Instance.DatabasePath)) {
					dbPath = ParadiseServerSettings.Instance.DatabasePath;
				} else {
					dbPath = Path.Combine(CurrentDirectory, ParadiseServerSettings.Instance.DatabasePath);
				}

				Log.Info($"Database path: {dbPath}");

				Database = new LiteDatabase(dbPath);
				Database.Pragma("UTC_DATE", true);
			} catch (Exception e) {
				Log.Error($"Failed to connect to database: {e.Message}");
				Log.Debug(e);

				DatabaseError?.Invoke(null, new ErrorEventArgs(e));

				return DatabaseOperationResult.GenericError;
			}

			Log.Info($"Database opened.");
			DatabaseOpened?.Invoke(null, new EventArgs());

			return DatabaseOperationResult.OpenOk;
		}

		public static DatabaseOperationResult ReloadDatabase() {
			DatabaseOperationResult result = default;

			if (Database != null) {
				result = DisposeDatabase();
			}

			if (result != DatabaseOperationResult.CloseOk) return result;

			if (Database == null) {
				result = OpenDatabase();
			}

			if (result == DatabaseOperationResult.OpenOk) {
				Log.Info($"Finished reloading database tables.");
			}

			return result;
		}
	}
}
