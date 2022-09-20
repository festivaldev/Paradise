using System;

namespace Paradise.WebServices {
	internal class CreditsCommand : IParadiseCommand {
		public string Command => "credits";
		public string[] Alias => new string[] { };

		public string Description => "Adds or removes credits from a player's wallet.";
		public string HelpString => $"{Command}\t\t{Description}";

		public void Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "add": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						CommandHandler.WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int credits)) {
						CommandHandler.WriteLine("Invalid parameter: credits");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						CommandHandler.WriteLine($"Failed to add credit(s) to wallet: Could not find player wallet.");
						return;
					}

					credits = Math.Abs(credits);
					memberWallet.Credits += credits;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					CommandHandler.WriteLine($"Successfully added {credits} credit(s) to wallet");

					break;
				}
				case "remove": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						CommandHandler.WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int credits)) {
						CommandHandler.WriteLine("Invalid parameter: credits");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						CommandHandler.WriteLine($"Failed to remove credit(s) from wallet: Could not find player wallet.");
						return;
					}

					credits = Math.Min(memberWallet.Credits, Math.Abs(credits));
					memberWallet.Credits -= credits;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					CommandHandler.WriteLine($"Successfully removed {credits} credit(s) from wallet");

					break;
				}
				default:
					CommandHandler.WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}

		public void PrintUsageText() {
			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine("  add <cmid> <amount>\t\tAdds the specified amount of credits to a players's wallet.");
			CommandHandler.WriteLine("  remove <cmid> <amount>\tRemoves the specified amount of credits from a players's wallet.");
		}
	}

	internal class PointsCommand : IParadiseCommand {
		public string Command => "points";
		public string[] Alias => new string[] { };

		public string Description => "Adds or removes points from a player's wallet.";
		public string HelpString => $"{Command}\t\t{Description}";

		public void Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "add": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						CommandHandler.WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int points)) {
						CommandHandler.WriteLine("Invalid parameter: points");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						CommandHandler.WriteLine($"Failed to add credit(s) to wallet: Could not find player wallet.");
						return;
					}

					points = Math.Abs(points);
					memberWallet.Points += points;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					CommandHandler.WriteLine($"Successfully added {points} point(s) to wallet");

					break;
				}
				case "remove": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						CommandHandler.WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int points)) {
						CommandHandler.WriteLine("Invalid parameter: points");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						CommandHandler.WriteLine($"Failed to remove credit(s) from wallet: Could not find player wallet.");
						return;
					}

					points = Math.Min(memberWallet.Points, Math.Abs(points));
					memberWallet.Points -= points;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					CommandHandler.WriteLine($"Successfully removed {points} point(s) from wallet");

					break;
				}
				default:
					CommandHandler.WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}

		public void PrintUsageText() {
			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine("  add <cmid> <amount>\t\tAdds the specified amount of points to a players's wallet.");
			CommandHandler.WriteLine("  remove <cmid> <amount>\tRemoves the specified amount of points from a players's wallet.");
		}
	}
}
