using System.Xml.Serialization;

namespace Paradise.WebServices {
	public class ParadiseSettings {
		[XmlElement]
		public bool EnableSSL;

		[XmlElement]
		public string WebServiceHostName = "localhost";

		[XmlElement]
		public int WebServicePort = 8080;

		[XmlElement]
		public string WebServicePrefix = "UberStrike.DataCenter.WebService.CWS.";

		[XmlElement]
		public string WebServiceSuffix = "Contract.svc";

		[XmlElement]
		public string FileServerHostName = "localhost";

		[XmlElement]
		public int FileServerPort = 8081;
	}
}