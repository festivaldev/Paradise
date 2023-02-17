using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.WebServices {
	internal class BanCommand : ParadiseCommand {
		public static new string Command => "ban";
		public static new IEnumerable<string> Aliases => new string[] { };

		public override string Description => "Bans a player permanently.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			$"Usage: {Command} <cmid> <duration> <reason>"
		};

		public BanCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 2) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				WriteLine("Invalid parameter: cmid");
				return;
			}

			if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var profileToBan) || profileToBan == null) {
				WriteLine("Could not ban user: Profile not found.");
				return;
			}

			if (!int.TryParse(arguments[1], out int duration)) {
				WriteLine("Invalid parameter: duration");
				return;
			}

			string reason = string.Join(" ", arguments.ToList().Skip(2).Take(arguments.Length - 1));

			if (DatabaseManager.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.ExpireTime > DateTime.UtcNow && _.TargetCmid == profileToBan.Cmid) != null) {
				WriteLine("Could not ban user: User is already banned.");
				return;
			}

			DatabaseManager.ModerationActions.Insert(new ModerationAction {
				ModerationFlag = ModerationFlag.Banned,
				SourceCmid = 0,
				SourceName = "ServerConsole",
				TargetCmid = profileToBan.Cmid,
				TargetName = profileToBan.Name,
				ActionDate = DateTime.UtcNow,
				ExpireTime = duration == 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMinutes(duration),
				Reason = reason,
			});

			if (duration == 0) {
				WriteLine($"User has been banned permanently (reason: {reason})");
			} else {
				WriteLine($"User has been banned for {duration} minute(s) (reason: {reason})");
			}
		}
	}

	internal class UnbanCommand : ParadiseCommand {
		public static new string Command => "unban";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Unbans a player.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			$"Usage: {Command} <cmid>"
		};

		public UnbanCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				WriteLine("Invalid parameter: cmid");
				return;
			}

			if (!(DatabaseManager.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.ExpireTime > DateTime.UtcNow && _.TargetCmid == cmid) is var bannedMember) || bannedMember == null) {
				WriteLine("Could not unban user: User is not currently banned.");
				return;
			}

			DatabaseManager.ModerationActions.DeleteMany(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == cmid);

			WriteLine("User has been unbanned successfully.");
		}
	}
}
