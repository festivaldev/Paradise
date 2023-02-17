using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Paradise.WebServices {
	public interface IParadiseCommand {
		string Command { get; }
		string[] Alias { get; }
		string Description { get; }
		string HelpString { get; }

		void PrintUsageText();
		void Run(string[] arguments);
	}

	public abstract class ParadiseCommand {
		public static string Command { get; }
		public static string[] Aliases { get; }

		public abstract string Description { get; }
		public abstract string HelpString { get; }
		public abstract string[] UsageText { get; }

		public Guid CommandGuid { get; private set; }

		public ParadiseCommand(Guid guid) {
			this.CommandGuid = guid;
		}

		public abstract void Run(string[] arguments);

		protected void WriteLine(string text) {
			Console.WriteLine(text);

			CommandHandler.CommandOutput?.Invoke(this, new CommandOutputArgs {
				Text = text
			});
		}

		protected void PrintUsageText() {
			foreach (var line in UsageText) {
				WriteLine(line);
			}
		}
	}

	public class CommandOutputArgs : EventArgs {
		public string Text { get; set; }
		public bool Inline { get; set; }
	}

	public class CommandCompletedArgs : EventArgs {
		public Guid Guid { get; set; }
	}

	public class CommandHandler {
		public static EventHandler<CommandCompletedArgs> CommandCompleted;
		public static EventHandler<CommandOutputArgs> CommandOutput;

		public static List<Type> Commands = new List<Type>();

		public static void HandleCommand(string command, string[] arguments, Guid? guid = null) {
			if (guid == null) {
				guid = Guid.NewGuid();
			}

			foreach (var type in Commands) {
				if (((string)type.GetProperty("Command").GetValue(null)).Equals(command, StringComparison.OrdinalIgnoreCase) ||
					((string[])type.GetProperty("Aliases").GetValue(null)).Contains(command, StringComparer.OrdinalIgnoreCase)) {


					var invoker = (ParadiseCommand)Activator.CreateInstance(type, new object[] { guid });

					invoker.Run(arguments);

					CommandCompleted?.Invoke(invoker, new CommandCompletedArgs {
						Guid = invoker.CommandGuid
					});

					return;
				}
			}

			if (!string.IsNullOrWhiteSpace(command)) {
				CommandOutput?.Invoke(null, new CommandOutputArgs {
					Text = $"{command}: Unkown command."
				});
			}
		}
	}
}
