using Cmune.DataCenter.Common.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Paradise.WebServices.Services {
	internal class WalletCommand : ParadiseCommand {
		public static new string Command => "wallet";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Manages credits and points in a player's wallet.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  info <name>\t\t\tShows the current status of a player's wallet.",
			"  credits",
			"    add <name> <amount>\t\tAdds the specified amount of credits to a players's wallet.",
			"    remove <name> <amount>\tRemoves the specified amount of credits from a players's wallet.",
			"  points",
			"    add <name> <amount>\t\tAdds the specified amount of points to a players's wallet.",
			"    remove <name> <amount>\tRemoves the specified amount of points from a players's wallet."
		};

		public override MemberAccessLevel MinimumAccessLevel => MemberAccessLevel.Moderator;

		public WalletCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			if (arguments.Length < 2) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "info": {
					var searchString = arguments[1];

					if (searchString.Length < 3) {
						WriteLine("Search pattern must contain at least 3 characters.");
						return;
					}

					var profiles = DatabaseClient.GetProfiles(searchString);
					var wallets = DatabaseClient.MemberWallets.FindAll().ToList();

					if (profiles.Count() > 0) {
						WriteLine(" ----------------------------------------------------- ");
						WriteLine($"| {"Username",-18} | {"CMID",-10} | {"Credits",-7} | {"Points",-7} |");
						WriteLine("|-----------------------------------------------------|");

						foreach (var profile in profiles) {
							var wallet = wallets.First(_ => _.Cmid == profile.Cmid);
							WriteLine($"| {profile.Name,-18} | {profile.Cmid,-10} | {wallet.Credits,-7} | {wallet.Points,-7} |");
						}

						WriteLine(" ----------------------------------------------------- ");
					} else {
						WriteLine($"Could not find any player matching {searchString}");
					}
					break;
				}

				case "credits": {
					if (arguments.Length < 4) {
						PrintUsageText();
						return;
					}

					switch (arguments[1].ToLower()) {
						case "add": {
							var searchString = arguments[2];

							if (searchString.Length < 3) {
								WriteLine("Search pattern must contain at least 3 characters.");
								return;
							}

							var publicProfile = DatabaseClient.GetProfile(searchString);

							if (publicProfile == null) {
								WriteLine($"Failed to add credit(s) to wallet: Could not find player matching {searchString}.");
								return;
							}

							if (!int.TryParse(arguments[3], out int amount)) {
								WriteLine("Invalid parameter: amount");
								return;
							}

							if (!(DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == publicProfile.Cmid) is var memberWallet) || memberWallet == null) {
								WriteLine($"Failed to add credit(s) to wallet: Could not find player wallet.");
								return;
							}

							amount = Math.Abs(amount);
							memberWallet.Credits += amount;

							DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
							DatabaseClient.MemberWallets.Insert(memberWallet);

							WriteLine($"Successfully added {amount} credit(s) to wallet");

							break;
						}

						case "remove": {
							var searchString = arguments[2];

							if (searchString.Length < 3) {
								WriteLine("Search pattern must contain at least 3 characters.");
								return;
							}

							var publicProfile = DatabaseClient.GetProfile(searchString);

							if (publicProfile == null) {
								WriteLine($"Failed to add credit(s) to wallet: Could not find player matching {searchString}.");
								return;
							}

							if (!int.TryParse(arguments[3], out int amount)) {
								WriteLine("Invalid parameter: amount");
								return;
							}

							if (!(DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == publicProfile.Cmid) is var memberWallet) || memberWallet == null) {
								WriteLine($"Failed to remove credit(s) from wallet: Could not find player wallet.");
								return;
							}

							amount = Math.Min(memberWallet.Credits, Math.Abs(amount));
							memberWallet.Credits -= amount;

							DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
							DatabaseClient.MemberWallets.Insert(memberWallet);

							WriteLine($"Successfully removed {amount} credits(s) from wallet");

							break;
						}
						default:
							WriteLine($"{Command}: unknown command {arguments[0]}\n");
							break;
					}
					break;
				}

				case "points": {
					if (arguments.Length < 4) {
						PrintUsageText();
						return;
					}

					switch (arguments[1].ToLower()) {
						case "add": {
							var searchString = arguments[2];

							if (searchString.Length < 3) {
								WriteLine("Search pattern must contain at least 3 characters.");
								return;
							}

							var publicProfile = DatabaseClient.GetProfile(searchString);

							if (publicProfile == null) {
								WriteLine($"Failed to add point(s) to wallet: Could not find player matching {searchString}.");
								return;
							}

							if (!int.TryParse(arguments[3], out int amount)) {
								WriteLine("Invalid parameter: amount");
								return;
							}

							if (!(DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == publicProfile.Cmid) is var memberWallet) || memberWallet == null) {
								WriteLine($"Failed to add point(s) to wallet: Could not find player wallet.");
								return;
							}

							amount = Math.Abs(amount);
							memberWallet.Points += amount;

							DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
							DatabaseClient.MemberWallets.Insert(memberWallet);

							WriteLine($"Successfully added {amount} point(s) to wallet");

							break;
						}
						case "remove": {
							var searchString = arguments[2];

							if (searchString.Length < 3) {
								WriteLine("Search pattern must contain at least 3 characters.");
								return;
							}

							var publicProfile = DatabaseClient.GetProfile(searchString);

							if (publicProfile == null) {
								WriteLine($"Failed to add credit(s) to wallet: Could not find player matching {searchString}.");
								return;
							}

							if (!int.TryParse(arguments[3], out int amount)) {
								WriteLine("Invalid parameter: amount");
								return;
							}

							if (!(DatabaseClient.MemberWallets.FindOne(_ => _.Cmid == publicProfile.Cmid) is var memberWallet) || memberWallet == null) {
								WriteLine($"Failed to remove point(s) from wallet: Could not find player wallet.");
								return;
							}

							amount = Math.Min(memberWallet.Points, Math.Abs(amount));
							memberWallet.Points -= amount;

							DatabaseClient.MemberWallets.DeleteMany(_ => _.Cmid == memberWallet.Cmid);
							DatabaseClient.MemberWallets.Insert(memberWallet);

							WriteLine($"Successfully removed {amount} point(s) from wallet");

							break;
						}
						default:
							WriteLine($"{Command}: unknown command {arguments[0]}\n");
							break;
					}
					break;
				}

				default: break;
			}
		}
#pragma warning restore CS1998
	}
}
