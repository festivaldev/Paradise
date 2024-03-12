using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Paradise.Realtime.Server {
	public class ApplicationConfiguration {
		protected static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationConfiguration));


		[YamlMember]
		public string MasterHostname { get; set; } = "127.0.0.1";

		[YamlMember]
		public int WebServicePort { get; set; } = 8080;

		[YamlMember]
		public int FileServerPort { get; set; } = 8081;

		[YamlMember]
		public int SocketPort { get; set; } = 8082;

		[YamlMember]
		public string WebServiceEndpoint { get; set; } = "/2.0";

		[YamlMember]
		public string WebServicePrefix { get; set; } = "UberStrike.DataCenter.WebService.CWS.";

		[YamlMember]
		public string WebServiceSuffix { get; set; } = "Contract.svc";

		[YamlMember]
		public bool WebServiceUseTLS { get; set; } = false;

		[YamlMember]
		public RealtimeApplicationSettings CommApplicationSettings = new RealtimeApplicationSettings();

		[YamlMember]
		public RealtimeApplicationSettings GameApplicationSettings = new RealtimeApplicationSettings();

		[YamlMember]
		public bool EnableChatLog = true;

		#region Client Verification
		[YamlMember]
		public int HeartbeatInterval = 5;

		[YamlMember]
		public int HeartbeatTimeout = 5;

		[YamlMember]
		public bool EnableHashVerification = true;

		[YamlMember]
		public List<string> CompositeHashes = new List<string>();

		[YamlMember]
		public List<string> JunkHashes = new List<string>();
		#endregion

		#region Discord Settings
		[YamlMember]
		public bool DiscordChatIntegration = false;

		[YamlMember]
		public bool DiscordPlayerAnnouncements = false;

		[YamlMember]
		public bool DiscordGameAnnouncements = false;

		[YamlMember]
		public bool DiscordErrorLog = false;
		#endregion



		[YamlIgnore]
		public List<byte[]> CompositeHashBytes { get; } = new List<byte[]>();

		[YamlIgnore]
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
			if (hashes.Count == 0)
				return;

			const string ALLOWED_CHARS = "0123456789abcdef";

			foreach (var hash in hashes) {
				if (hash?.Length > 0) {
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
		[YamlMember]
		public Guid ApplicationIdentifier;

		[YamlMember]
		public string EncryptionPassPhrase;
	}
}
