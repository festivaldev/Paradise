namespace Paradise.WebServices {
	internal class DatabaseCommand : IParadiseCommand {
		public string Command => "database";
		public string[] Alias => new string[] { "db" };

		public string Description => "Controls the LiteDB database instance.";
		public string HelpString => $"{Command}\t{Description}";

		public void Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			var result = default(DatabaseOperationResult);

			switch (arguments[0].ToLower()) {
				case "close":
					result = DatabaseManager.DisposeDatabase();
					break;
				case "open":
					result = DatabaseManager.OpenDatabase();
					break;
				case "reload":
					result = DatabaseManager.ReloadDatabase();
					break;
				default:
					CommandHandler.WriteLine($"database: unknown command {arguments[0]}");
					return;

			}

			switch (result) {
				case DatabaseOperationResult.kDatabaseResultOpenOk:
					CommandHandler.WriteLine("Database opened successfully.");
					break;
				case DatabaseOperationResult.kDatabaseResultCloseOk:
					CommandHandler.WriteLine("Database opened successfully.");
					break;
				case DatabaseOperationResult.kDatabaseResultNotOpened:
					CommandHandler.WriteLine("Failed to close database: Database isn't open.");
					break;
				case DatabaseOperationResult.kDatabaseResultAlreadyOpened:
					CommandHandler.WriteLine("Failed to open database: Database has been opened already.");
					break;
				case DatabaseOperationResult.kDatabaseResultGenericError:
					CommandHandler.WriteLine("An error occured while opening/closing the database.");
					break;
			}
		}

		public void PrintUsageText() {
			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine("  close\t\tSaves the database and closes the instance.");
			CommandHandler.WriteLine("  open\t\tOpens a new database instance.");
			CommandHandler.WriteLine("  reload\tReloads the current database instance");
		}
	}
}
