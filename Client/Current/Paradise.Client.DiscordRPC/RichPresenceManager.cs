using DiscordRPC;
using DiscordRPC.Logging;
using System;

namespace Paradise.Client.DiscordRPC {
	internal class RichPresenceManager {
		private static DiscordRpcClient rpcClient;

		public static void Initialize() {
			Console.WriteLine($"Initializing DiscordRPC for application \"1071893834172223518\"");

			rpcClient = new DiscordRpcClient("1071893834172223518") {
				Logger = new ConsoleLogger() { Level = LogLevel.Warning }
			};

			rpcClient.OnReady += (sender, e) => {
				Console.WriteLine("Received Ready from user {0}", e.User.Username);
			};

			rpcClient.OnPresenceUpdate += (sender, e) => {
				Console.WriteLine("Received Update! {0}", e.Presence);
			};

			rpcClient.Initialize();
		}

		public static void SetPresence(RichPresenceSerializable presence) {
			try {
				if (presence.ClearPresence) {
					rpcClient.ClearPresence();
				} else {
					rpcClient.SetPresence(RichPresenceSerializable.Deserialize(presence));
				}
			} catch (Exception e) {
				Console.WriteLine($"Failed to set presence: {e}");
			}
		}
	}
}
