using System.Collections.Generic;
using System.Xml.Serialization;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.Client {
	public enum UpdateChannel {
		Stable,
		Beta
	}

	public class ParadiseSettings {
		[XmlElement]
		public string WebServiceBaseUrl;

		[XmlElement]
		public string WebServicePrefix;

		[XmlElement]
		public string WebServiceSuffix;

		[XmlElement]
		public string ImagePath;

		[XmlElement]
		public bool AutoUpdates = true;

		[XmlElement]
		public UpdateChannel UpdateChannel;

		[XmlElement]
		public string UpdateUrl;

		[XmlElement]
		public AuthenticateApplicationView ServerOverrides;
	}
}