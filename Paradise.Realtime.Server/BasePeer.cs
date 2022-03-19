using log4net;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Paradise.Realtime.Server {
	public class BasePeer : ClientPeer {
		private static readonly ILog Log = LogManager.GetLogger(nameof(BasePeer));

		private readonly ConcurrentDictionary<int, BaseOperationHandler> _opHandlers;

		public bool HasError { get; protected set; }
		public string AuthToken { get; protected set; }

		public BasePeer(InitRequest initRequest) : base(initRequest) {
			if (initRequest.ApplicationId != ApiVersion.Current && initRequest.ApplicationId != ApiVersion.Legacy) {
				Disconnect();
			}

			_opHandlers = new ConcurrentDictionary<int, BaseOperationHandler>();
		}

		public void Authenticate(string authToken, string magicHash) {
			AuthToken = authToken ?? throw new ArgumentNullException(nameof(authToken));

			if (magicHash == null) {
				throw new ArgumentNullException(nameof(magicHash));
			}

			Log.Info($"Received AuthenticationRequest! {authToken}:{magicHash} (at {RemoteIP}:{RemotePort})");
		}

		public void AddOperationHandler(BaseOperationHandler handler) {
			if (handler == null) {
				throw new ArgumentNullException(nameof(handler));
			}

			if (!_opHandlers.TryAdd(handler.Id, handler)) {
				throw new ArgumentException("Already contains a handler with the same handler ID.");
			}
		}

		public void RemoveOperationHandler(int handlerId) {
			var handler = default(BaseOperationHandler);
			_opHandlers.TryRemove(handlerId, out handler);
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail) {
			foreach (var opHandler in _opHandlers.Values) {
				try {
					opHandler.OnDisconnect(this, reasonCode, reasonDetail);
				} catch (Exception ex) {
					Log.Error($"Error while handling disconnection of peer -> {opHandler.GetType().Name}", ex);
				}
			}
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters) {
			/* 
                OperationRequest should contain 1 parameters.
                [0] -> (Int32 - OperationHandler ID) ->> (Byte[] - Data).
                Then we use OperationRequest.OperationCode & OperationHandler ID to,
                determine how to read stuff.
                Check if we've got enough parameters.
             */
			if (operationRequest.Parameters.Count < 1) {
				Log.Warn("Disconnecting client since its does not have enough parameters!");
				Disconnect();
				return;
			}

			var handlerId = operationRequest.Parameters.Keys.First();
			var handler = default(BaseOperationHandler);
			if (_opHandlers.TryGetValue(handlerId, out handler)) {
				var data = (byte[])operationRequest.Parameters[handlerId];
				using (var bytes = new MemoryStream(data)) {
					try {
						handler.OnOperationRequest(this, operationRequest.OperationCode, bytes);
					} catch (NotImplementedException ex) {
						var stackTrace = new System.Diagnostics.StackTrace(ex);
						Log.Warn($"Not Implemented: {handler.GetType().Name}:{stackTrace.GetFrame(0).GetMethod().Name}");
					} catch (Exception ex) {
						Log.Error($"Error while handling request {handler.GetType().Name}:{handlerId} -> OpCode: {operationRequest.OperationCode}", ex);
					}
				}
			} else {
				Log.Warn($"Unable to handle operation request -> operation handler not implemented: {handlerId}");
			}
		}

		public virtual void SendError(string message = "An error occured that forced UberStrike to halt.") {
			HasError = true;
		}
	}
}
