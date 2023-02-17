using System;

namespace Paradise.WebServices {
	internal class XpCommand : ParadiseCommand {
		public static new string Command => "xp";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Increases or decreases a player's level.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  give <cmid> <amount>\t\tAdds the specified amount of experience to increase a player's level.",
			"  take <cmid> <amount>\t\tRemoves the specified amount of experience to decrease a player's level."
		};

		public XpCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0]) {
				case "give": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int xpAmount)) {
						WriteLine("Invalid parameter: xp");
						return;
					}

					if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var publicProfile) || publicProfile == null) {
						WriteLine("Could not increase player experience: Profile not found.");
						return;
					}

					if (!(DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == publicProfile.Cmid) is var playerStatistics) || playerStatistics == null) {
						WriteLine("Could not increase player experience: Player statistics not found.");
						return;
					}

					xpAmount = Math.Abs(xpAmount);
					playerStatistics.Xp += xpAmount;
					playerStatistics.Level = XpPointsUtil.GetLevelForXp(playerStatistics.Xp);

					DatabaseManager.PlayerStatistics.DeleteMany(_ => _.Cmid == publicProfile.Cmid);
					DatabaseManager.PlayerStatistics.Insert(playerStatistics);

					WriteLine($"Successfully added {xpAmount} XP to player (total: {playerStatistics.Xp}, level: {playerStatistics.Level})");

					break;
				}
				case "take": {
					if (!int.TryParse(arguments[1], out int cmid)) {
						WriteLine("Invalid parameter: cmid");
						return;
					}

					if (!int.TryParse(arguments[2], out int xpAmount)) {
						WriteLine("Invalid parameter: xp");
						return;
					}

					if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var publicProfile) || publicProfile == null) {
						WriteLine("Could not decrease player experience: Profile not found.");
						return;
					}

					if (!(DatabaseManager.PlayerStatistics.FindOne(_ => _.Cmid == publicProfile.Cmid) is var playerStatistics) || publicProfile == null) {
						WriteLine("Could not decrease player experience: Player statistics not found.");
						return;
					}

					xpAmount = Math.Min(playerStatistics.Xp, Math.Abs(xpAmount));
					playerStatistics.Xp -= xpAmount;
					playerStatistics.Level = XpPointsUtil.GetLevelForXp(playerStatistics.Xp);

					DatabaseManager.PlayerStatistics.DeleteMany(_ => _.Cmid == publicProfile.Cmid);
					DatabaseManager.PlayerStatistics.Insert(playerStatistics);

					WriteLine($"Successfully removed {xpAmount} XP from player (total: {playerStatistics.Xp}, level: {playerStatistics.Level})");

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
	}
}
