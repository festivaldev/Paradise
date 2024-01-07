using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server {
	public class PeerConfiguration {
		public bool HashVerificationEnabled;

		public int HeartbeatInterval;
		public int HeartbeatTimeout;

		public IReadOnlyList<byte[]> CompositeHashes { get; }
		public IReadOnlyList<byte[]> JunkHashes { get; }

		public PeerConfiguration(int heartbeatInterval, int heartbeatTimeout, IReadOnlyList<byte[]> compositeHashes, IReadOnlyList<byte[]> junkHashes) {
			CompositeHashes = compositeHashes ?? throw new ArgumentNullException(nameof(compositeHashes));
			JunkHashes = junkHashes ?? throw new ArgumentNullException(nameof(junkHashes));

			if (heartbeatInterval <= 0) {
				throw new ArgumentOutOfRangeException(nameof(heartbeatInterval));
			}

			HeartbeatInterval = heartbeatInterval;

			if (heartbeatTimeout <= 0) {
				throw new ArgumentOutOfRangeException(nameof(heartbeatTimeout));
			}

			HeartbeatTimeout = heartbeatTimeout;
		}
	}
}
