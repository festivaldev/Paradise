using log4net;
using Newtonsoft.Json;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Comm {
	public class CommApplication : BaseRealtimeApplication {
		protected static readonly new ILog Log = LogManager.GetLogger("CommLog");
		protected ServiceHost ServiceHost;

		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			return new CommPeer(initRequest);
		}

		protected override void OnSetup() {
			Log.Info($"Started CommServer.");

			ServiceHost = new ServiceHost(typeof(CommApplicationMonitoring));
			ServiceHost.AddServiceEndpoint(typeof(IParadiseMonitoring), new NetNamedPipeBinding(), "net.pipe://localhost/NewParadise.Monitoring.Comm");

			Task.Factory.StartNew(() => {
				Task.Delay(300).Wait();

				ServiceHost.Open();
			});
		}

		protected override void OnTearDown() {
			Log.Info($"Stopped CommServer.");

			ServiceHost.Abort();
		}
	}

	public class CommApplicationMonitoring : IParadiseMonitoring {
		protected static readonly new ILog Log = LogManager.GetLogger("CommLog");

		public string GetStatus() {
			try {
				return JsonConvert.SerializeObject(new Dictionary<string, object> {
					["peers"] = LobbyManager.Instance.Peers.Select(peer => peer.Actor.ActorInfo)
				});
			} catch (Exception e) {
				Log.Error(e);

				return $"{{\"error\": \"{e.Message}\"}}";
			}
		}

		public byte Ping() {
			return 0x1;
		}
	}
}
