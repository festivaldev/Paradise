using Cmune.DataCenter.Common.Entities;
using System;
using System.Threading.Tasks;

namespace Paradise.WebServices.Services {
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

		public override MemberAccessLevel MinimumAccessLevel => MemberAccessLevel.Moderator;

		public XpCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0]) {
				case "give": {
					var searchString = arguments[1];

					if (searchString.Length < 3) {
						WriteLine("Search pattern must contain at least 3 characters.");
						return;
					}

					var publicProfile = DatabaseClient.GetProfile(searchString);

					if (publicProfile == null) {
						WriteLine($"Failed to increase player experience: Could not find player matching {searchString}.");
						return;
					}

					if (!int.TryParse(arguments[2], out int xpAmount)) {
						WriteLine("Invalid parameter: xp");
						return;
					}

					if (!(DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == publicProfile.Cmid) is var playerStatistics) || playerStatistics == null) {
						WriteLine("Failed to increase player experience: Player statistics not found.");
						return;
					}

					xpAmount = Math.Abs(xpAmount);
					playerStatistics.Xp += xpAmount;
					playerStatistics.Level = XpPointsUtil.GetLevelForXp(playerStatistics.Xp);

					DatabaseClient.PlayerStatistics.DeleteMany(_ => _.Cmid == publicProfile.Cmid);
					DatabaseClient.PlayerStatistics.Insert(playerStatistics);

					WriteLine($"Successfully added {xpAmount} XP to player (total: {playerStatistics.Xp}, level: {playerStatistics.Level})");

					break;
				}
				case "take": {
					var searchString = arguments[1];

					if (searchString.Length < 3) {
						WriteLine("Search pattern must contain at least 3 characters.");
						return;
					}

					var publicProfile = DatabaseClient.GetProfile(searchString);

					if (publicProfile == null) {
						WriteLine($"Failed to decrease player experience: Could not find player matching {searchString}.");
						return;
					}

					if (!int.TryParse(arguments[2], out int xpAmount)) {
						WriteLine("Invalid parameter: xp");
						return;
					}

					if (!(DatabaseClient.PlayerStatistics.FindOne(_ => _.Cmid == publicProfile.Cmid) is var playerStatistics) || publicProfile == null) {
						WriteLine("Failed to decrease player experience: Player statistics not found.");
						return;
					}

					xpAmount = Math.Min(playerStatistics.Xp, Math.Abs(xpAmount));
					playerStatistics.Xp -= xpAmount;
					playerStatistics.Level = XpPointsUtil.GetLevelForXp(playerStatistics.Xp);

					DatabaseClient.PlayerStatistics.DeleteMany(_ => _.Cmid == publicProfile.Cmid);
					DatabaseClient.PlayerStatistics.Insert(playerStatistics);

					WriteLine($"Successfully removed {xpAmount} XP from player (total: {playerStatistics.Xp}, level: {playerStatistics.Level})");

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
#pragma warning restore CS1998
	}
}
