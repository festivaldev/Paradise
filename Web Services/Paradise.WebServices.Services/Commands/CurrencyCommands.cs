using System;

namespace Paradise.WebServices {
	internal class CreditsCommand : ParadiseCommand {
		public static new string Command => "credits";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Adds or removes credits from a player's wallet.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  add <cmid> <amount>\t\tAdds the specified amount of credits to a players's wallet.",
			"  remove <cmid> <amount>\tRemoves the specified amount of credits from a players's wallet."
		};

		public CreditsCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "add": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int amount)) {
						WriteLine("Invalid parameter: amount");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						WriteLine($"Failed to add credit(s) to wallet: Could not find player wallet.");
						return;
					}

					amount = Math.Abs(amount);
					memberWallet.Credits += amount;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					WriteLine($"Successfully added {amount} credit(s) to wallet");

					break;
				}
				case "remove": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int amount)) {
						WriteLine("Invalid parameter: amount");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						WriteLine($"Failed to remove credit(s) from wallet: Could not find player wallet.");
						return;
					}

					amount = Math.Min(memberWallet.Credits, Math.Abs(amount));
					memberWallet.Credits -= amount;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					WriteLine($"Successfully removed {amount} credit(s) from wallet");

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
	}

	internal class PointsCommand : ParadiseCommand {
		public static new string Command => "points";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Adds or removes points from a player's wallet.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  add <cmid> <amount>\t\tAdds the specified amount of points to a players's wallet.",
			"  remove <cmid> <amount>\tRemoves the specified amount of points from a players's wallet."
		};

		public PointsCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "add": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int amount)) {
						WriteLine("Invalid parameter: amount");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						WriteLine($"Failed to add credit(s) to wallet: Could not find player wallet.");
						return;
					}

					amount = Math.Abs(amount);
					memberWallet.Points += amount;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					WriteLine($"Successfully added {amount} point(s) to wallet");

					break;
				}
				case "remove": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int amount)) {
						WriteLine("Invalid parameter: amount");
						return;
					}

					if (!(DatabaseManager.MemberWallets.FindOne(_ => _.Cmid == cmid) is var memberWallet) || memberWallet == null) {
						WriteLine($"Failed to remove credit(s) from wallet: Could not find player wallet.");
						return;
					}

					amount = Math.Min(memberWallet.Points, Math.Abs(amount));
					memberWallet.Points -= amount;

					DatabaseManager.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
					DatabaseManager.MemberWallets.Insert(memberWallet);

					WriteLine($"Successfully removed {amount} point(s) from wallet");

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
	}
}
