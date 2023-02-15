using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.WebServices {
	public interface IParadiseCommand {
		string Command { get; }
		string[] Alias { get; }
		string Description { get; }
		string HelpString { get; }

		void PrintUsageText();
		void Run(string[] arguments);
	}

	public class CommandCallbackArgs : EventArgs {
		public string CommandOutput { get; set; }
		public bool SingleLine { get; set; }
	}

	public class CommandHandler {
		public static EventHandler<CommandCallbackArgs> CommandCallback;

		public static List<IParadiseCommand> Commands = new List<IParadiseCommand>();

		public static void HandleCommand(string command, string[] arguments) {
			foreach (var cmd in Commands) {
				if (cmd.Command.Equals(command, StringComparison.OrdinalIgnoreCase) ||
					cmd.Alias.ToList().Contains(command, StringComparer.OrdinalIgnoreCase)) {
					cmd.Run(arguments);
					return;
				}
			}

			if (!string.IsNullOrWhiteSpace(command)) {
				WriteLine($"{command}: Unkown command.");
			}
		}

		public static void Write(string message) {
			Console.WriteLine(message);

			CommandCallback.Invoke(null, new CommandCallbackArgs {
				CommandOutput = message,
				SingleLine = true
			});
		}

		public static void WriteLine(string message) {
			Console.WriteLine(message);

			CommandCallback.Invoke(null, new CommandCallbackArgs {
				CommandOutput = message
			});
		}
	}
}
