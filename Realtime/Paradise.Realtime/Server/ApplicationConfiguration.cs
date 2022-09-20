using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Paradise.Realtime.Server {
	public class ApplicationConfiguration {
		public static readonly ApplicationConfiguration Default = new ApplicationConfiguration {
			WebServiceBaseUrl = "http://localhost:8080/2.0/",
			WebServicePrefix = "UberStrike.DataCenter.WebService.CWS.",
			WebServiceSuffix = "Contract.svc"
		};

		[XmlElement]
		public string WebServiceBaseUrl;

		[XmlElement]
		public string WebServicePrefix;

		[XmlElement]
		public string WebServiceSuffix;

		[XmlElement]
		public int HeartbeatInterval = 5;

		[XmlElement]
		public int HeartbeatTimeout = 5;

		[XmlElement]
		private List<string> CompositeHashes = new List<string>();

		[XmlElement]
		private List<string> JunkHashes = new List<string>();

		public List<byte[]> CompositeHashBytes { get; } = new List<byte[]>();

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
			const string ALLOWED_CHARS = "0123456789abcdef";

			foreach (var hash in hashes) {
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
