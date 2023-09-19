using System.Xml.Serialization;

namespace Paradise.WebServices.Discord {
	[XmlRoot("ParadiseDiscordSettings")]
	public class DiscordSettings {
		[XmlElement]
		public bool EnableDiscordIntegration;

		[XmlElement]
		public string Token;

		[XmlElement]
		public bool ChatIntegration = true;

		[XmlElement]
		public bool CommandIntegration = true;

		[XmlElement]
		public bool PlayerJoinAnnouncements = true;

		[XmlElement]
		public bool PlayerLeaveAnnouncements = true;

		[XmlElement]
		public bool RoomOpenAnnouncements = true;

		[XmlElement]
		public bool RoomCloseAnnouncements = false;

		[XmlElement]
		public bool ErrorLog = false;

		[XmlElement]
		public ulong GuildId;

		[XmlElement]
		public ulong ChatChannelId;

		[XmlElement]
		public ulong CommandChannelId;

		[XmlElement]
		public string ChatWebHookUrl;

		[XmlElement]
		public string PlayerAnnouncementWebHookUrl;

		[XmlElement]
		public string GameAnnouncementWebHookUrl;

		[XmlElement]
		public string ErrorLogWebHookUrl;
	}
}
