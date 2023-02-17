using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;

namespace Paradise.WebServices {
	internal class DeopCommand : ParadiseCommand {
		public static new string Command => "deop";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Resets a user's permission level.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			$"Usage: {Command} <cmid>"
		};

		public DeopCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				WriteLine("Invalid parameter: cmid");
				return;
			}

			if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var targetProfile) || targetProfile == null) {
				WriteLine("Could not reset user permission level: Profile not found.");
			}

			if (targetProfile.AccessLevel == MemberAccessLevel.Default) {
				WriteLine("Could not reset user permission level: Invalid data.");
				return;
			}

			targetProfile.AccessLevel = MemberAccessLevel.Default;

			DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
			DatabaseManager.PublicProfiles.Insert(targetProfile);

			WriteLine("User permission level has been reset successfully.");
		}
	}

	internal class OpCommand : ParadiseCommand {
		public static new string Command => "op";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Sets a user's permission level.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText {
			get {
				var lines = new List<string> {
					$"{Command}: {Description}",
					$"Usage: {Command} <cmid> <level>"
				};

				var values = new List<string>();
				foreach (var value in Enum.GetValues(typeof(MemberAccessLevel))) {
					values.Add($"{value} = {(int)value}");
				}

				lines.Add(string.Join("; ", values));

				return lines.ToArray();
			}
		}

		public OpCommand(Guid guid) : base(guid) { }

		public override void Run(string[] arguments) {
			if (arguments.Length < 2) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				WriteLine("Invalid parameter: cmid");
				return;
			}

			if (!Enum.TryParse(arguments[1], out MemberAccessLevel level)) {
				WriteLine("Invalid parameter: level");
				return;
			}

			if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var targetProfile) || targetProfile == null) {
				WriteLine("Could not set user permission level: Profile not found.");
				return;
			}

			if (level == MemberAccessLevel.Default || targetProfile.AccessLevel == level) {
				WriteLine("Could not set user permission level: Invalid data.");
				return;
			}

			targetProfile.AccessLevel = level;

			DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
			DatabaseManager.PublicProfiles.Insert(targetProfile);

			WriteLine("User permission level has been set successfully.");
		}
	}
}
