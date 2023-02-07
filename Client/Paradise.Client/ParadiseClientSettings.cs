using System.IO;
using System.Xml.Serialization;
using UberStrike.DataCenter.Common.Entities;
using UnityEngine;

namespace Paradise.Client {
	public enum UpdateChannel {
		Stable,
		Beta
	}

	public class ParadiseClientSettings {
		[XmlIgnore]
		public static string SettingsFilename => Path.Combine(Application.dataPath, "Paradise.Settings.Client.xml");

		[XmlElement]
		public bool EnableDiscordRichPresence = true;

		[XmlElement]
		public string WebServiceBaseUrl = "https://paradise.festival.tf:5053/2.0/";

		[XmlElement]
		public string WebServicePrefix = "UberStrike.DataCenter.WebService.CWS.";

		[XmlElement]
		public string WebServiceSuffix = "Contract.svc";

		[XmlElement]
		public string ImagePath = "https://paradise.festival.tf:5054/images/";

		[XmlElement]
		public bool AutoUpdates = true;

		[XmlElement]
		public UpdateChannel UpdateChannel = UpdateChannel.Stable;

		[XmlElement]
		public string UpdateUrl = "https://paradise.festival.tf:5054/updates/";

		[XmlElement]
		public AuthenticateApplicationView ServerOverrides;
	}
}