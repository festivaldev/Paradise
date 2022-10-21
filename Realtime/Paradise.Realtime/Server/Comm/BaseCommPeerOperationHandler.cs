using log4net;
using Paradise.Core.Serialization;
using System.IO;

namespace Paradise.Realtime.Server.Comm {
	public abstract class BaseCommPeerOperationHandler : BaseOperationHandler<CommPeer> {
		private static readonly ILog Log = LogManager.GetLogger("CommLog");

		public override int Id => 1;

		public abstract void OnAuthenticationRequest(CommPeer peer, string authToken, string magicHash);
		public abstract void OnSendHeartbeatResponse(CommPeer peer, string authToken, string responseHash);

		public override void OnOperationRequest(CommPeer peer, byte opCode, MemoryStream bytes) {
			Log.Debug($"CommPeer.OnOperationRequest peer: {peer}, opCode: {(ILobbyRoomOperationsType)opCode}({opCode})");

			var operation = (ICommPeerOperationsType)opCode;
			switch (operation) {
				case ICommPeerOperationsType.AuthenticationRequest:
					AuthenticationRequest(peer, bytes);
					break;

				case ICommPeerOperationsType.SendHeartbeatResponse:
					SendHeartbeatResponse(peer, bytes);
					break;
			}
		}

		private void AuthenticationRequest(CommPeer peer, MemoryStream bytes) {
			var authToken = StringProxy.Deserialize(bytes);
			var magicHash = StringProxy.Deserialize(bytes);

			DebugOperation(authToken, magicHash);

			OnAuthenticationRequest(peer, authToken, magicHash);
		}

		private void SendHeartbeatResponse(CommPeer peer, MemoryStream bytes) {
			var authToken = StringProxy.Deserialize(bytes);
			var responseHash = StringProxy.Deserialize(bytes);

			DebugOperation(authToken, responseHash);

			OnSendHeartbeatResponse(peer, authToken, responseHash);
		}

		private void DebugOperation(params object[] data) {
#if DEBUG
			Log.Info($"{GetType().Name}:{new StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
#endif
		}
	}
}
