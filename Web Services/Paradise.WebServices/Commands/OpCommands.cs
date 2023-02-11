using Discord.WebSocket;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;

namespace Paradise.WebServices {
	internal class DeopCommand : IParadiseCommand {
		public string Command => "deop";
		public string[] Alias => new string[] { };

		public string Description => "Resets a user's permission level.";
		public string HelpString => $"{Command}\t\t{Description}";

		private SocketMessage DiscordMessage;

		public void Run(string[] arguments, SocketMessage discordMessage) {
			DiscordMessage = discordMessage;

			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				CommandHandler.WriteLine("Invalid parameter: cmid", DiscordMessage);
				return;
			}

			if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var targetProfile) || targetProfile == null) {
				CommandHandler.WriteLine("Could not reset user permission level: Profile not found.", DiscordMessage);
			}

			if (targetProfile.AccessLevel == MemberAccessLevel.Default) {
				CommandHandler.WriteLine("Could not reset user permission level: Invalid data.", DiscordMessage);
				return;
			}

			targetProfile.AccessLevel = MemberAccessLevel.Default;

			DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
			DatabaseManager.PublicProfiles.Insert(targetProfile);

			CommandHandler.WriteLine("User permission level has been reset successfully.", DiscordMessage);

			DiscordMessage = null;
		}

		public void PrintUsageText() {
			if (DiscordMessage != null) {
				PrintUsageTextDiscord();
				return;
			}

			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine($"Usage: {Command} <cmid>");
		}

		public void PrintUsageTextDiscord() {
			CommandHandler.WriteLine($"{Command}: {Description}\n" + 
									 $"Usage: {Command} <cmid>",
				DiscordMessage);
		}
	}

	internal class OpCommand : IParadiseCommand {
		public string Command => "op";
		public string[] Alias => new string[] { };

		public string Description => "Sets a user's permission level.";
		public string HelpString => $"{Command}\t\t{Description}";

		private SocketMessage DiscordMessage;

		public void Run(string[] arguments, SocketMessage discordMessage) {
			DiscordMessage = discordMessage;

			if (arguments.Length < 2) {
				PrintUsageText();
				return;
			}

			if (!int.TryParse(arguments[0], out int cmid)) {
				CommandHandler.WriteLine("Invalid parameter: cmid", DiscordMessage);
				return;
			}

			if (!Enum.TryParse(arguments[1], out MemberAccessLevel level)) {
				CommandHandler.WriteLine("Invalid parameter: level", DiscordMessage);
				return;
			}

			if (!(DatabaseManager.PublicProfiles.FindOne(_ => _.Cmid == cmid) is var targetProfile) || targetProfile == null) {
				CommandHandler.WriteLine("Could not set user permission level: Profile not found.", DiscordMessage);
				return;
			}

			if (level == MemberAccessLevel.Default || targetProfile.AccessLevel == level) {
				CommandHandler.WriteLine("Could not set user permission level: Invalid data.", DiscordMessage);
				return;
			}

			targetProfile.AccessLevel = level;

			DatabaseManager.PublicProfiles.DeleteMany(_ => _.Cmid == targetProfile.Cmid);
			DatabaseManager.PublicProfiles.Insert(targetProfile);

			CommandHandler.WriteLine("User permission level has been set successfully.", DiscordMessage);

			DiscordMessage = null;
		}

		public void PrintUsageText() {
			if (DiscordMessage != null) {
				PrintUsageTextDiscord();
				return;
			}

			CommandHandler.WriteLine($"{Command}: {Description}");
			CommandHandler.WriteLine($"Usage: {Command} <cmid> <level>");

			var values = new List<string>();
			foreach (var value in Enum.GetValues(typeof(MemberAccessLevel))) {
				values.Add($"{value} = {(int)value}");
			}

			CommandHandler.WriteLine(string.Join("; ", values));
		}

		public void PrintUsageTextDiscord() {
			var message = $"{Command}: {Description}\n" +
						  $"Usage: {Command} <cmid> <level>\n";

			var values = new List<string>();
			foreach (var value in Enum.GetValues(typeof(MemberAccessLevel))) {
				values.Add($"{value} = {(int)value}");
			}

			message += string.Join("; ", values);

			CommandHandler.WriteLine(message, DiscordMessage);
		}
	}
}
