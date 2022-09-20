using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.WebServices {
	internal class XpCommand : IParadiseCommand {
		public string Command => "xp";
		public string[] Alias => new string[] { };

		public string Description => "Increases or decreases a player's level.";
		public string HelpString => $"{Command}\t\t{Description}";

		public void Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0]) {
				case "give": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						CommandHandler.WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int xpAmount)) {
						CommandHandler.WriteLine("Invalid parameter: xp");
						return;
					}

					if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var publicProfile) || publicProfile == null) {
						CommandHandler.WriteLine("Could not increase player experience: Profile not found.");
						return;
					}

					if (!(DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == publicProfile.Cmid) is var playerStatistics) || playerStatistics == null) {
						CommandHandler.WriteLine("Could not increase player experience: Player statistics not found.");
						return;
					}

					xpAmount = Math.Abs(xpAmount);
					playerStatistics.Xp += xpAmount;
					playerStatistics.Level = XpPointsUtil.GetLevelForXp(playerStatistics.Xp);

					DatabaseManager.PlayerStatistics.DeleteMany(_ => _.Cmid == publicProfile.Cmid);
					DatabaseManager.PlayerStatistics.Insert(playerStatistics);

					CommandHandler.WriteLine($"Successfully added {xpAmount} XP to player (total: {playerStatistics.Xp}, level: {playerStatistics.Level})");

					break;
				}
				case "take": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						CommandHandler.WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int xpAmount)) {
						CommandHandler.WriteLine("Invalid parameter: xp");
						return;
					}

					if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var publicProfile) || publicProfile == null) {
						CommandHandler.WriteLine("Could not decrease player experience: Profile not found.");
						return;
					}

					if (!(DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == publicProfile.Cmid) is var playerStatistics) || publicProfile == null) {
						CommandHandler.WriteLine("Could not decrease player experience: Player statistics not found.");
						return;
					}

					xpAmount = Math.Min(playerStatistics.Xp, Math.Abs(xpAmount));
					playerStatistics.Xp -= xpAmount;
					playerStatistics.Level = XpPointsUtil.GetLevelForXp(playerStatistics.Xp);

					DatabaseManager.PlayerStatistics.DeleteMany(_ => _.Cmid == publicProfile.Cmid);
					DatabaseManager.PlayerStatistics.Insert(playerStatistics);

					CommandHandler.WriteLine($"Successfully removed {xpAmount} XP from player (total: {playerStatistics.Xp}, level: {playerStatistics.Level})");

					break;
				}
				default:
					CommandHandler.WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}

		public void PrintUsageText() {
			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine("  give <cmid> <amount>\t\tAdds the specified amount of experience to increase a player's level.");
			CommandHandler.WriteLine("  take <cmid> <amount>\t\tRemoves the specified amount of experience to decrease a player's level.");
		}
	}
}
