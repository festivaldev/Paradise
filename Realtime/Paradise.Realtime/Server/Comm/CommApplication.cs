using log4net;
using Newtonsoft.Json;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Comm {
	public class CommApplication : BaseRealtimeApplication {
		protected static readonly new ILog Log = LogManager.GetLogger("CommLog");

		public string Identifier = Guid.NewGuid().ToString();

		protected System.Timers.Timer MonitoringTimer;

		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			return new CommPeer(initRequest);
		}

		protected override void OnSetup() {
			MonitoringTimer = new System.Timers.Timer(5000);
			MonitoringTimer.Elapsed += delegate {
				ApplicationWebServiceClient.Instance.PublishCommMonitoringData(Identifier, GetStatus());
			};
			MonitoringTimer.Start();

			Log.Info($"Started CommServer[{Identifier}].");
		}

		protected override void OnTearDown() {
			Log.Info($"Stopping CommServer[{Identifier}]...");

			MonitoringTimer.Stop();
		}

		private string GetStatus() {
			try {
				return JsonConvert.SerializeObject(new Dictionary<string, object> {
					["peers"] = LobbyManager.Instance.Peers.Select(peer => peer.Actor.ActorInfo)
				});
			} catch (Exception e) {
				Log.Error(e);

				return $"{{\"error\": \"{e.Message}\"}}";
			}
		}
	}
}
