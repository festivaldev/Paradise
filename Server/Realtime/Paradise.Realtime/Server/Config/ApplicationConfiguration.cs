using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Paradise.Realtime.Server {
	public class ApplicationConfiguration {
		protected static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationConfiguration));

		public static readonly ApplicationConfiguration Default = new ApplicationConfiguration {
			MasterServerUrl = "http://localhost",
			WebServicePort = 8080,
			WebServiceEndpoint = "/2.0",
			WebServicePrefix = "UberStrike.DataCenter.WebService.CWS.",
			WebServiceSuffix = "Contract.svc",
			FileServerPort = 8081,
			TCPCommPort = 8082
		};

		[XmlElement]
		public string MasterServerUrl;

		[XmlElement]
		public int WebServicePort;

		[XmlElement]
		public string WebServiceEndpoint;

		[XmlElement]
		public string WebServicePrefix;

		[XmlElement]
		public string WebServiceSuffix;

		[XmlElement]
		public int FileServerPort;

		[XmlElement]
		public int TCPCommPort;

		[XmlElement]
		public RealtimeApplicationSettings CommApplicationSettings;

		[XmlElement]
		public RealtimeApplicationSettings GameApplicationSettings;

		[XmlElement]
		public bool EnableChatLog = true;

		[XmlElement]
		public int HeartbeatInterval = 5;

		[XmlElement]
		public int HeartbeatTimeout = 5;

		[XmlElement]
		public bool EnableHashVerification = true;

		[XmlIgnore]
		public List<string> CompositeHashes = new List<string>();

		[XmlIgnore]
		public List<string> JunkHashes = new List<string>();

		[XmlElement]
		public bool DiscordChatIntegration = false;

		[XmlElement]
		public bool DiscordPlayerAnnouncements = false;

		[XmlElement]
		public bool DiscordGameAnnouncements = false;

		[XmlElement]
		public bool DiscordErrorLog = false;



		[XmlIgnore]
		public List<byte[]> CompositeHashBytes { get; } = new List<byte[]>();

		[XmlIgnore]
		public List<byte[]> JunkHashBytes { get; } = new List<byte[]>();

		public void Validate() {
			if (HeartbeatInterval < 0) {
				throw new FormatException("HeartbeatInterval cannot be less than 0.");
			} else if (HeartbeatInterval == 0) {
				HeartbeatInterval = 5;
			}

			if (HeartbeatTimeout < 0) {
				throw new FormatException("HeartbeatTimeout cannot be less than 0.");
			} else if (HeartbeatTimeout == 0) {
				HeartbeatTimeout = 5;
			}

			ValidateHashes(CompositeHashes, CompositeHashBytes);
			ValidateHashes(JunkHashes, JunkHashBytes);
		}

		private void ValidateHashes(List<string> hashes, List<byte[]> hashBytes) {
			if (hashes.Count == 0) return;

			const string ALLOWED_CHARS = "0123456789abcdef";

			foreach (var hash in hashes) {
				if (hash.Length > 0) {
					if (hash.Length != 64) {
						throw new FormatException($"Hash must be 64 characters long (got {hash.Length})");
					}

					foreach (var c in hash) {
						if (!ALLOWED_CHARS.Contains(c.ToString())) {
							throw new FormatException("Hash contains illegal character(s)");
						}
					}

					hashBytes.Add(Encoding.ASCII.GetBytes(hash));
				}
			}
		}
	}

	public class RealtimeApplicationSettings {
		[XmlElement]
		public string ApplicationIdentifier;

		[XmlElement]
		public string EncryptionPassPhrase;
	}
}
