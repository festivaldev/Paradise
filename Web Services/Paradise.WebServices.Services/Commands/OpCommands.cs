using Cmune.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paradise.WebServices.Services {
	internal class DeopCommand : ParadiseCommand {
		public static new string Command => "deop";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Resets a user's permission level.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			$"Usage: {Command} <name>"
		};

		public DeopCommand(Guid guid) : base(guid) { }

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

			var targetProfile = DatabaseClient.GetProfile(searchString);

			if (targetProfile == null) {
				WriteLine($"Failed to reset user permission level: Could not find player matching {searchString}.");
				return;
			}

			if (targetProfile.AccessLevel == MemberAccessLevel.Default) {
				WriteLine("Failed to reset user permission level: Invalid data.");
				return;
			}

			targetProfile.AccessLevel = MemberAccessLevel.Default;

			DatabaseClient.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
			DatabaseClient.PublicProfiles.Insert(targetProfile);

			WriteLine("User permission level has been reset successfully.");
		}
#pragma warning restore CS1998
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
					$"Usage: {Command} <name> <level>"
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

#pragma warning disable CS1998
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

			var targetProfile = DatabaseClient.GetProfile(searchString);

			if (targetProfile == null) {
				WriteLine($"Failed to set user permission level: Could not find player matching {searchString}.");
				return;
			}

			if (!Enum.TryParse(arguments[1], out MemberAccessLevel level)) {
				WriteLine("Invalid parameter: level");
				return;
			}

			if (level == MemberAccessLevel.Default || targetProfile.AccessLevel == level) {
				WriteLine("Failed to set user permission level: Invalid data.");
				return;
			}

			targetProfile.AccessLevel = level;

			DatabaseClient.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
			DatabaseClient.PublicProfiles.Insert(targetProfile);

			WriteLine("User permission level has been set successfully.");
		}
#pragma warning restore CS1998
	}
}
