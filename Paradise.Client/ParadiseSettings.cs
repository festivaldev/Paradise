using System.Xml.Serialization;

namespace Paradise.Client {
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
		public string UpdateChannel;

		[XmlElement]
		public string UpdateUrl;
	}
}