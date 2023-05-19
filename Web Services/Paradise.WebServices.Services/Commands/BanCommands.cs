using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paradise.WebServices.Services {
	internal class BanCommand : ParadiseCommand {
		public static new string Command => "ban";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Bans a player for a specified duration.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			$"Usage: {Command} <name> <duration> <reason>"
		};

		public override MemberAccessLevel MinimumAccessLevel => MemberAccessLevel.Moderator;

		public BanCommand(Guid guid) : base(guid) { }

		public override async Task Run(string[] arguments) {
			if (arguments.Length < 2) {
				PrintUsageText();
				return;
			}

			var searchString = arguments[0];

			if (searchString.Length < 3) {
				WriteLine("Search pattern must contain at least 3 characters.");
				return;
			}

			var publicProfile = DatabaseClient.GetProfile(searchString);

			if (publicProfile == null) {
				WriteLine($"Failed to ban player: Could not find player matching {searchString}.");
				return;
			}

			if (!(DatabaseClient.PublicProfiles.FindOne(_ => _.Cmid == publicProfile.Cmid) is var profileToBan) || profileToBan == null) {
				WriteLine("Failed to ban player: Profile not found.");
				return;
			}

			if (!int.TryParse(arguments[1], out int duration)) {
				WriteLine("Invalid parameter: duration");
				return;
			}

			string reason = string.Join(" ", arguments.ToList().Skip(2).Take(arguments.Length - 1));

			if (DatabaseClient.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.ExpireTime > DateTime.UtcNow && _.TargetCmid == profileToBan.Cmid) != null) {
				WriteLine("Failed to ban player: Player is already banned.");
				return;
			}

			DatabaseClient.ModerationActions.Insert(new ModerationAction {
				ModerationFlag = ModerationFlag.Banned,
				SourceCmid = 0,
				SourceName = "ServerConsole",
				TargetCmid = profileToBan.Cmid,
				TargetName = profileToBan.Name,
				ActionDate = DateTime.UtcNow,
				ExpireTime = duration == 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMinutes(duration),
				Reason = reason,
			});

			await ParadiseService.Instance.SocketServer.SendToCommServer(TcpSocket.PacketType.BanPlayer, new Dictionary<string, object> {
				["TargetCmid"] = profileToBan.Cmid,
				["Duration"] = duration,
				["ExpireTime"] = duration == 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMinutes(duration),
				["Reason"] = reason
			});

			if (duration == 0) {
				WriteLine($"Player has been banned permanently (reason: {reason})");
			} else {
				WriteLine($"Player has been banned for {duration} minute(s) (reason: {reason})");
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
			$"Usage: {Command} <name>"
		};

		public override MemberAccessLevel MinimumAccessLevel => MemberAccessLevel.Moderator;

		public UnbanCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			var searchString = arguments[0];

			if (searchString.Length < 3) {
				WriteLine("Search pattern must contain at least 3 characters.");
				return;
			}

			var publicProfile = DatabaseClient.GetProfile(searchString);

			if (publicProfile == null) {
				WriteLine($"Failed to unban player: Could not find player matching {searchString}.");
				return;
			}

			if (!(DatabaseClient.ModerationActions.FindOne(_ => _.ModerationFlag == ModerationFlag.Banned && _.ExpireTime > DateTime.UtcNow && _.TargetCmid == publicProfile.Cmid) is var bannedMember) || bannedMember == null) {
				WriteLine("Failed to unban player: Player is not currently banned.");
				return;
			}

			DatabaseClient.ModerationActions.DeleteMany(_ => _.ModerationFlag == ModerationFlag.Banned && _.TargetCmid == publicProfile.Cmid);

			WriteLine("User has been unbanned successfully.");
		}
#pragma warning restore CS1998
	}
}
