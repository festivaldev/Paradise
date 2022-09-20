using System.Xml.Serialization;

namespace Paradise {
	public class ParadiseServerSettings {
		public static ParadiseServerSettings Instance { get; private set; }

		public ParadiseServerSettings() {
			Instance = this;
		}

		[XmlElement]
		public bool EnableSSL;

		[XmlElement]
		public string SSLCertificateName;

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

		[XmlElement]
		public string FileServerRoot = "wwwroot";

		[XmlElement]
		public string EncryptionInitVector = "aaaabbbbccccdddd";

		[XmlElement]
		public string EncryptionPassPhrase = "voJRIh4LEA/lnk19/HucN9qywkxsYNHHE5H410vTRrw=";
	}
}