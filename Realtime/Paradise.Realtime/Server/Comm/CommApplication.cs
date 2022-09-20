using Photon.SocketServer;

namespace Paradise.Realtime.Server.Comm {
	public class CommApplication : BaseRealtimeApplication {
		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			return new CommPeer(initRequest);
		}

		protected override void OnSetup() {
			Log.Info($"Started CommServer.");
		}

		protected override void OnTearDown() {
			Log.Info($"Stopped CommServer.");
		}
	}
}
