using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server {
	public class PeerConfiguration {
		public string WebServiceBaseUrl { get; }

		public int HeartbeatInterval { get; }
		public int HeartbeatTimeout { get; }

		public IReadOnlyList<byte[]> CompositeHashes { get; }
		public IReadOnlyList<byte[]> JunkHashes { get; }

		public PeerConfiguration() { }

		public PeerConfiguration(string webServiceBaseUrl, int heartbeatInterval, int heartbeatTimeout, IReadOnlyList<byte[]> compositeHashes, IReadOnlyList<byte[]> junkHashes) {
			WebServiceBaseUrl = webServiceBaseUrl ?? throw new ArgumentNullException(nameof(webServiceBaseUrl));

			CompositeHashes = compositeHashes ?? throw new ArgumentNullException(nameof(compositeHashes));
			JunkHashes = junkHashes ?? throw new ArgumentNullException(nameof(junkHashes));

			if (heartbeatInterval <= 0) {
				throw new ArgumentOutOfRangeException(nameof(heartbeatInterval));
			} else {
				HeartbeatInterval = heartbeatInterval;
			}

			if (heartbeatTimeout <= 0) {
				throw new ArgumentOutOfRangeException(nameof(heartbeatTimeout));
			} else {
				HeartbeatTimeout = heartbeatTimeout;
			}
		}
	}
}
