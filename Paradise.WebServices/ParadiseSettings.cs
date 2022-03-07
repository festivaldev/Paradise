using System.Xml.Serialization;

namespace Paradise.WebServices {
	public class ParadiseSettings {
		[XmlElement]
		public string WebServiceBaseUrl = "http://localhost/2.0/";

		[XmlElement]
		public string ImagePath = "http://localhost/";

		[XmlElement]
		public string WebServicePrefix = "UberStrike.DataCenter.WebService.CWS.";

		[XmlElement]
		public string WebServiceSuffix = "Contract.svc";
	}
}