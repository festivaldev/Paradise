namespace Paradise.WebServices.Discord {
	public class DiscordUser {
		public int Cmid { get; set; }
		public ulong DiscordUserId { get; set; }
		public string Nonce { get; set; }

		public override string ToString() {
			return $"<@{DiscordUserId}>";
		}
	}
}
