using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.WebServices {
	internal class ServerCommand : ParadiseCommand {
		public static new string Command => "server";
		public static new string[] Aliases => new string[] { };

		public override string Description => "Manage server credentials.";
		public override string HelpString => $"{Command}\t\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  generate\t\tGenerates credentials for a new server."
		};

		public ServerCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			if (arguments.Length < 1) {
				PrintUsageText();
				return;
			}

			switch (arguments[0].ToLower()) {
				case "generate": {
					var serverId = Guid.NewGuid();
					var passphrase = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes($"{serverId}_{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds()}"));

					WriteLine($"Server ID: {serverId}");
					WriteLine($"Passphrase: {Convert.ToBase64String(passphrase)}");

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
#pragma warning restore CS1998
	}
}
