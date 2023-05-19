using System;
using System.Linq;
using System.Threading.Tasks;

namespace Paradise.WebServices {
	internal class HelpCommand : ParadiseCommand {
		public static new string Command => "help";
		public static new string[] Aliases => new string[] { "h" };

		public override string Description => "Shows this help text. (Alias: h)";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] { };

		public HelpCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			WriteLine("Available commands:" + Environment.NewLine);
			WriteLine("clear\t\tClears the console, obviously.");
			WriteLine("quit\t\tQuits the application (or closes the console if running in GUI mode). (Alias: q)");

			foreach (var type in CommandHandler.Commands.OrderBy(_ => _.Name).ToList()) {
				var cmd = (ParadiseCommand)Activator.CreateInstance(type, new object[] { Guid.Empty });
				WriteLine(cmd.HelpString);
			}
		}
#pragma warning restore CS1998
	}
}
