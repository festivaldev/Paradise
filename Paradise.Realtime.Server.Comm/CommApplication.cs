using Photon.SocketServer;

namespace Paradise.Realtime.Server.Comm {
	public class CommApplication : BaseApplication {
		protected override PeerBase OnCreatePeer(InitRequest initRequest) {
			Log.Info($"Accepted new connection at {initRequest.RemoteIP}:{initRequest.RemotePort}");

			return new CommPeer(initRequest);
		}

		protected override void OnSetup() {
			Log.Info("Started CommServer.");
		}

		protected override void OnTearDown() {
			Log.Info("Stopped CommServer.");
		}
	}
}
