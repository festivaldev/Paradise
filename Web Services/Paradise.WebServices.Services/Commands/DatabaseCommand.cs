using System;

namespace Paradise.WebServices {
	internal class DatabaseCommand : ParadiseCommand {
		public static new string Command => "database";
		public static new string[] Aliases => new string[] { "db" };

		public override string Description => "Controls the LiteDB database instance.";
		public override string HelpString => $"{Command}\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  close\t\tSaves the database and closes the instance.",
			"  open\t\tOpens a new database instance.",
			"  reload\tReloads the current database instance",
		};

		public DatabaseCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
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
					WriteLine($"database: unknown command {arguments[0]}");
					return;

			}

			switch (result) {
				case DatabaseOperationResult.OpenOk:
					WriteLine("Database opened successfully.");
					break;
				case DatabaseOperationResult.CloseOk:
					WriteLine("Database opened successfully.");
					break;
				case DatabaseOperationResult.NotOpened:
					WriteLine("Failed to close database: Database isn't open.");
					break;
				case DatabaseOperationResult.AlreadyOpened:
					WriteLine("Failed to open database: Database has been opened already.");
					break;
				case DatabaseOperationResult.GenericError:
					WriteLine("An error occured while opening/closing the database.");
					break;
			}
		}
	}
}
