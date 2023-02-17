namespace Paradise.WebServices.Discord {
	public class Plugin : ParadiseServicePlugin {
		private DiscordClient discordClient;

		public override void OnLoad() {
			discordClient = new DiscordClient();
		}

		public override void OnStart() {
			base.OnStart();

			discordClient.Connect();
		}

		public override void OnStop() {
			discordClient.Disconnect();
		}
	}
}
