using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using UberStrike.Realtime.UnitySdk;

namespace Paradise.Realtime.Server {
	public enum HeartbeatState {
		Ok,
		Waiting,
		Failed
	}

	public abstract class BasePeer : ClientPeer {
		private readonly ConcurrentDictionary<int, object> OperationHandlers = new ConcurrentDictionary<int, object>();

		private static readonly string[] SupportedApplications = new[] { ApiVersion.Current };

		protected PeerConfiguration Configuration { get; }
		public int HeartbeatInterval { get; set; }
		public int HeartbeatTimeout { get; set; }

		private string heartbeat;
		private DateTime nextHeartbeatTime;
		private DateTime heartbeatExpireTime;
		private HeartbeatState heartbeatState;

		public BasePeer(InitRequest initRequest) : base(initRequest) {
			if (!SupportedApplications.Contains(initRequest.ApplicationId)) {
				Disconnect();
				return;
			}

			if (initRequest.UserData is PeerConfiguration config) {
				Configuration = config;
				HeartbeatInterval = config.HeartbeatInterval;
				HeartbeatTimeout = config.HeartbeatTimeout;
			}

			if (Configuration.CompositeHashes.Count > 0 || Configuration.JunkHashes.Count > 0) {
				nextHeartbeatTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
			}
		}
	}
}
