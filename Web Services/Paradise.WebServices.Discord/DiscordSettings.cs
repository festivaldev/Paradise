using System.Xml.Serialization;

namespace Paradise.WebServices.Discord {
	[XmlRoot("ParadiseDiscordSettings")]
	public class DiscordSettings {
		[XmlElement]
		public bool EnableDiscordIntegration;

		[XmlElement]
		public string Token;

		[XmlElement]
		public string WebHookUrl;

		[XmlElement]
		public bool ChatIntegration = true;

		[XmlElement]
		public bool CommandIntegration = true;

		[XmlElement]
		public bool PlayerAnnouncements = true;

		[XmlElement]
		public bool GameAnnouncements = true;

		[XmlElement]
		public bool ErrorLog = false;

		[XmlElement]
		public ulong GuildId;

		[XmlElement]
		public ulong ChatChannelId;

		[XmlElement]
		public ulong CommandChannelId;

		[XmlElement]
		public ulong PlayerAnnouncementChannelId;

		[XmlElement]
		public ulong GameAnnouncementChannelId;

		[XmlElement]
		public ulong ErrorLogChannelId;
	}
}
