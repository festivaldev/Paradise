using System;

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

			switch (arguments[0].ToLower()) {
				case "close":
					DatabaseManager.DisposeDatabase();
					break;
				case "open":
					DatabaseManager.OpenDatabase();
					break;
				case "reload":
					DatabaseManager.ReloadDatabase();
					break;
				default:
					CommandHandler.WriteLine($"database: unknown command {arguments[0]}");
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
