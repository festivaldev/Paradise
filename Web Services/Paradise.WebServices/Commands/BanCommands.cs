using Paradise.DataCenter.Common.Entities;
using System;
using System.Linq;

namespace Paradise.WebServices {
	internal class BanCommands : IParadiseCommand {
		public string Command => "ban";
		public string[] Alias => new string[] { };

		public string Description => "Bans a player permanently.";
		public string HelpString => $"{Command}\t\t{Description}";

		public void Run(string[] arguments) {
			if (arguments.Length < 2) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				CommandHandler.WriteLine("Invalid parameter: cmid");
				return;
			}

			if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var profileToBan) || profileToBan == null) {
				CommandHandler.WriteLine("Could not ban user: Profile not found.");
				return;
			}

			string reason = string.Join(" ", arguments.ToList().Skip(1).Take(arguments.Length - 1));

			if (DatabaseManager.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == profileToBan.Cmid) != null) {
				CommandHandler.WriteLine("Could not ban user: User is already permanently banned.");
				return;
			}

			DatabaseManager.ModerationActions.Insert(new ModerationAction {
				ModerationFlag = ModerationFlag.Banned,
				SourceCmid = 0,
				SourceName = "ServerConsole",
				TargetCmid = profileToBan.Cmid,
				TargetName = profileToBan.Name,
				ActionDate = DateTime.UtcNow,
				ExpireTime = DateTime.MaxValue,
				Reason = reason,
			});

			CommandHandler.WriteLine($"User has been banned permanently (reason: {reason})");
		}

		public void PrintUsageText() {
			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine($"Usage: {Command} <cmid> <reason>");
		}
	}

	internal class UnbanCommand : IParadiseCommand {
		public string Command => "unban";
		public string[] Alias => new string[] { };

		public string Description => "Unbans a player.";
		public string HelpString => $"{Command}\t\t{Description}";

		public void Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				CommandHandler.WriteLine("Invalid parameter: cmid");
				return;
			}

			if (!(DatabaseManager.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == cmid) is var bannedMember) || bannedMember == null) {
				CommandHandler.WriteLine("Could not unban user: User is not currently banned.");
				return;
			}

			DatabaseManager.ModerationActions.DeleteMany(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == cmid);

			CommandHandler.WriteLine("User has been unbanned successfully.");
		}

		public void PrintUsageText() {
			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine($"Usage: {Command} <cmid>");
		}
	}
}
