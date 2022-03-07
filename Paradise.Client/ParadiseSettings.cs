using System.Xml.Serialization;

namespace Paradise.Client {
	public class ParadiseSettings {
		[XmlElement]
		public string WebServiceBaseUrl;

		[XmlElement]
		public string ImagePath;
	}
}