using DiscordRPC;
using DiscordRPC.Logging;
using System;
using System.ServiceModel;

namespace Paradise.Client.DiscordRPC {
	[ServiceContract]
	public interface RpcServiceHost {
		[OperationContract]
		void ClearPresence();

		[OperationContract]
		void SetPresence(string details, string state);

		[OperationContract]
		void SetPresenceWithEndTimestamp(string details, string state, DateTime end);

		[OperationContract]
		void SetPresenceWithStartEnd(string details, string state, DateTime start, DateTime end);
	}

	internal class RichPresenceManager : RpcServiceHost {
		private static DiscordRpcClient rpcClient;

		public static void Initialize() {
			Console.WriteLine($"Initializing DiscordRPC for application {"1071893834172223518"}");
			rpcClient = new DiscordRpcClient("1071893834172223518");

			rpcClient.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

			rpcClient.OnReady += (sender, e) => {
				Console.WriteLine("Received Ready from user {0}", e.User.Username);
			};

			rpcClient.OnPresenceUpdate += (sender, e) => {
				Console.WriteLine("Received Update! {0}", e.Presence);
			};

			rpcClient.Initialize();
		}

		public void ClearPresence() {
			rpcClient.ClearPresence();
		}

		public void SetPresence(string details, string state) {
			rpcClient.SetPresence(new RichPresence() {
				Details = details,
				State = state,
				Assets = new Assets() {
					LargeImageKey = "uberstrike"
				}
			});
		}

		public void SetPresenceWithEndTimestamp(string details, string state, DateTime end) {
			rpcClient.SetPresence(new RichPresence() {
				Details = details,
				State = state,
				Assets = new Assets() {
					LargeImageKey = "uberstrike"
				},
				Timestamps = new Timestamps() {
					Start = DateTime.UtcNow,
					End = end
				}
			});
		}

		public void SetPresenceWithStartEnd(string details, string state, DateTime start, DateTime end) {
			rpcClient.SetPresence(new RichPresence() {
				Details = details,
				State = state,
				Assets = new Assets() {
					LargeImageKey = "uberstrike"
				},
				Timestamps = new Timestamps() {
					Start = start,
					End = end,
				}
			});
		}
	}
}
