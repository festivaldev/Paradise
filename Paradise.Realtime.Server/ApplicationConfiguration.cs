using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server {
	public class ApplicationConfiguration {
		[JsonRequired]
		[JsonProperty("serviceBaseURL")]
		public string ServiceBaseURL { get; private set; }

		[JsonRequired]
		[JsonProperty("serviceAuth")]
		public string ServiceAuth { get; private set; }

		[JsonProperty("heartbeatInterval")]
		public int HeartbeatInterval { get; private set; }

		[JsonProperty("heartbeatTimeout")]
		public int HeartbeatTimeout { get; private set; }

		[JsonIgnore]
		public List<byte[]> CompositeHashBytes { get; } = new List<byte[]>();

		[JsonIgnore]
		public List<byte[]> JunkHashBytes { get; } = new List<byte[]>();

		[JsonRequired]
		[JsonProperty("compositeHashes")]
		private List<string> CompositeHashes;

		[JsonRequired]
		[JsonProperty("junkHashes")]
		private List<string> JunkHashes;


		public static readonly ApplicationConfiguration Default = new ApplicationConfiguration {
			ServiceBaseURL = "http://localhost:5053/2.0/",
			ServiceAuth = null,
			HeartbeatInterval = 5,
			HeartbeatTimeout = 5,
			CompositeHashes = new List<string>(),
			JunkHashes = new List<string>()
		};

		public void Check() {
			if (HeartbeatInterval < 0 || HeartbeatTimeout < 0)
				throw new FormatException("HeartbeatInterval or HeartbeatTimeout cannot be less than 0.");

			if (HeartbeatInterval == 0)
				HeartbeatInterval = 5;
			if (HeartbeatTimeout == 0)
				HeartbeatTimeout = 5;

			CheckHashes(CompositeHashes, CompositeHashBytes);
			CheckHashes(JunkHashes, JunkHashBytes);
		}

		private void CheckHashes(List<string> hashes, List<byte[]> hashBytes) {
			const string VALID_CHARS = "0123456789abcdef";

			foreach (var hash in hashes) {
				if (hash.Length != 64)
					throw new FormatException("Hash string must be exactly 64 characters long!");

				foreach (var c in hash) {
					if (!VALID_CHARS.Contains(c.ToString()))
						throw new FormatException("Hash contains invalid characters!");

					hashBytes.Add(Encoding.ASCII.GetBytes(hash));
				}
			}
		}
	}
}
