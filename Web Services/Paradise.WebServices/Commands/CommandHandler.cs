using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.WebServices {
	internal interface IParadiseCommand {
		string Command { get; }
		string[] Alias { get; }
		string Description { get; }
		string HelpString { get; }

		void PrintUsageTest();
		void PrintUsageTextDiscord();
		void Run(string[] arguments, SocketMessage discordMessage = null);
	}

	public class CommandCallbackArgs : EventArgs {
		public string CommandOutput { get; set; }
		public bool SingleLine { get; set; }
		public SocketMessage DiscordMessage { get; set; }
	}

	internal class CommandHandler {
		public static EventHandler<CommandCallbackArgs> CommandCallback;

		public static readonly List<IParadiseCommand> Commands = new List<IParadiseCommand> {
			new BanCommands(),
			new CreditsCommand(),
			new DatabaseCommand(),
			new DeopCommand(),
			new InventoryCommand(),
			new OpCommand(),
			new PointsCommand(),
			new ServiceCommand(),
			new UnbanCommand(),
			new XpCommand()
		};

		public static void HandleCommand(string command, string[] arguments, SocketMessage discordMessage = null) {
			foreach (var cmd in Commands) {
				if (cmd.Command.Equals(command, StringComparison.OrdinalIgnoreCase) ||
					cmd.Alias.ToList().Contains(command, StringComparer.OrdinalIgnoreCase)) {
					cmd.Run(arguments, discordMessage);
					return;
				}
			}

			if (!string.IsNullOrWhiteSpace(command)) {
				WriteLine($"{command}: Unkown command.", discordMessage);
			}
		}

		public static void Write(string message, SocketMessage discordMessage = null) {
			if (discordMessage == null) {
				Console.WriteLine(message);
			}

			CommandCallback.Invoke(null, new CommandCallbackArgs {
				CommandOutput = message,
				SingleLine = true,
				DiscordMessage = discordMessage
			});
		}

		public static void WriteLine(string message, SocketMessage discordMessage = null) {
			if (discordMessage == null) {
				Console.WriteLine(message);
			}

			CommandCallback.Invoke(null, new CommandCallbackArgs {
				CommandOutput = message,
				DiscordMessage = discordMessage
			});
		}
	}
}
