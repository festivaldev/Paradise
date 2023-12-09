using Cmune.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paradise.WebServices {
	public abstract class ParadiseCommand {
		public class CommandOutputArgs : EventArgs {
			public Guid InvocationId { get; set; }
			public string Text { get; set; }
			public bool Inline { get; set; }
		}

		public static string Command { get; }
		public static string[] Aliases { get; }

		public abstract string Description { get; }
		public abstract string HelpString { get; }
		public abstract string[] UsageText { get; }

		public virtual MemberAccessLevel MinimumAccessLevel { get; } = MemberAccessLevel.Admin;

		public Guid InvocationId { get; private set; }

		public ParadiseCommand(Guid invocationId) {
			this.InvocationId = invocationId;
		}

		public abstract Task Run(string[] arguments);

		public EventHandler<CommandOutputArgs> CommandOutput;
		private readonly List<string> OutputBuffer = new List<string>();
		public string Output {
			get {
				return string.Join(Environment.NewLine, OutputBuffer);
			}
		}

		public void ClearOutputBuffer() {
			OutputBuffer.Clear();
		}

		protected void WriteLine(string text) {
			OutputBuffer.Add(text);

			CommandOutput?.Invoke(this, new CommandOutputArgs {
				InvocationId = this.InvocationId,
				Text = text
			});
		}

		protected void Write(string text) {
			if (OutputBuffer.Count > 0) {
				OutputBuffer[OutputBuffer.Count - 1] = string.Concat(OutputBuffer.Last(), text);
			} else {
				OutputBuffer.Add(text);
			}

			CommandOutput?.Invoke(this, new CommandOutputArgs {
				InvocationId = this.InvocationId,
				Text = text,
				Inline = true
			});
		}

		protected void PrintUsageText() {
			foreach (var line in UsageText) {
				WriteLine(line);
			}
		}
	}

	public static class CommandHandler {
		public static List<Type> Commands { get; private set; } = new List<Type>();

		public static Guid HandleCommand(string command, string[] args, Guid invocationId = default, Action<string, bool> outputCallback = null, Action<ParadiseCommand, bool, string> completedCallback = null) {
			if (invocationId == null || invocationId.Equals(Guid.Empty)) {
				invocationId = Guid.NewGuid();
			}

			return Task.Run(async () => {
				foreach (var type in Commands) {
					if ((type.GetProperty("Command").GetValue(null) as string).Equals(command, StringComparison.OrdinalIgnoreCase) ||
						(type.GetProperty("Aliases").GetValue(null) as string[]).Contains(command, StringComparer.OrdinalIgnoreCase)) {
						var invoker = (ParadiseCommand)Activator.CreateInstance(type, new object[] { invocationId });

						invoker.CommandOutput += (sender, e) => {
							outputCallback?.Invoke(e.Text, e.Inline);
						};

						await invoker.Run(args);

						completedCallback?.Invoke(invoker, true, null);
						invoker.ClearOutputBuffer();

						return invocationId;
					}
				}

				if (!string.IsNullOrWhiteSpace(command)) {
					completedCallback?.Invoke(null, false, $"{command}: Unknown command.");
				}

				return invocationId;
			}).Result;
		}

		public static Guid HandleUserCommand(string command, string[] args, PublicProfileView user, Guid invocationId = default, Action<string, bool> outputCallback = null, Action<ParadiseCommand, bool, string> completedCallback = null) {
			if (invocationId == null || invocationId.Equals(Guid.Empty)) {
				invocationId = Guid.NewGuid();
			}

			return Task.Run(async () => {
				foreach (var type in Commands) {
					if ((type.GetProperty("Command").GetValue(null) as string).Equals(command, StringComparison.OrdinalIgnoreCase) ||
						(type.GetProperty("Aliases").GetValue(null) as string[]).Contains(command, StringComparer.OrdinalIgnoreCase)) {
						var invoker = (ParadiseCommand)Activator.CreateInstance(type, new object[] { invocationId });

						if (user.AccessLevel < invoker.MinimumAccessLevel) {
							completedCallback?.Invoke(null, false, $"{command}: Insufficient rights to execute command.");

							return invocationId;
						}

						invoker.CommandOutput += (sender, e) => {
							outputCallback?.Invoke(e.Text, e.Inline);
						};

						await invoker.Run(args);

						completedCallback?.Invoke(invoker, true, null);
						invoker.ClearOutputBuffer();

						return invocationId;
					}
				}

				if (!string.IsNullOrWhiteSpace(command)) {
					completedCallback?.Invoke(null, false, $"{command}: Unknown command.");
				}

				return invocationId;
			}).Result;
		}
	}
}
