using log4net;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Paradise.Realtime.Server {
	public enum HeartbeatState {
		Ok,
		Waiting,
		Failed
	}

	public abstract class BasePeer : ClientPeer {
		private static readonly ILog Log = LogManager.GetLogger(nameof(BasePeer));

		private readonly ConcurrentDictionary<int, BaseOperationHandler> _opHandlers;

		public bool HasError { get; protected set; }
		public string AuthToken { get; protected set; }

		protected PeerConfiguration Configuration { get; }
		public int HeartbeatInterval { get; set; }
		public int HeartbeatTimeout { get; set; }

		private string heartbeat;
		private DateTime nextHeartbeatTime;
		private DateTime heartbeatExpireTime;
		private HeartbeatState heartbeatState;

		public BasePeer(InitRequest initRequest) : base(initRequest) {
			if (initRequest.ApplicationId != ApiVersion.Current && initRequest.ApplicationId != ApiVersion.Legacy) {
				Disconnect();
			}

			_opHandlers = new ConcurrentDictionary<int, BaseOperationHandler>();

			if (initRequest.UserData is PeerConfiguration config) {
				Configuration = config;
				HeartbeatInterval = config.HeartbeatInterval;
				HeartbeatTimeout = config.HeartbeatTimeout;
			}

			if (Configuration.JunkHashes.Count > 0) {
				nextHeartbeatTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
			}
		}

		public void Authenticate(string authToken, string magicHash) {
			AuthToken = authToken ?? throw new ArgumentNullException(nameof(authToken));

			if (magicHash == null) {
				throw new ArgumentNullException(nameof(magicHash));
			}

			Log.Debug($"Received AuthenticationRequest! {authToken}:{magicHash} (at {RemoteIP}:{RemotePort})");
		}

		public void Tick() {
			switch (heartbeatState) {
				case HeartbeatState.Ok:
					if (DateTime.UtcNow >= nextHeartbeatTime) {
						Heartbeat();
					}

					break;
				case HeartbeatState.Waiting:
					if (DateTime.UtcNow >= heartbeatExpireTime) {
						Disconnect();
					}

					break;
				case HeartbeatState.Failed:
					SendError();

					break;
				default: break;
			}
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

		public void Heartbeat() {
			//if (Configuration.JunkHashes.Count == 0) return;

			heartbeat = GenerateHeartbeat();
			heartbeatExpireTime = DateTime.UtcNow.AddSeconds(HeartbeatTimeout);
			heartbeatState = HeartbeatState.Waiting;

//#if DEBUG
//			Log.Debug($"Heartbeat({heartbeat}) with {HeartbeatTimeout}s timeout, expires at {heartbeatExpireTime}");
//#endif

			SendHeartbeat(heartbeat);
		}

		public abstract void SendHeartbeat(string hash);

		public bool CheckHeartbeat(string responseHash) {
			if (responseHash == null) {
				throw new ArgumentNullException(nameof(responseHash));
			}

//#if DEBUG
//			Log.Info($"HeartbeatCheck({responseHash})");
//#endif

			if (heartbeat == null) {
				Log.Error("Heartbeat was null while checking.");
				return false;
			}

			if (true) {
				// We'll just pass through the heartbeat response for now
				// as a way to check if the peer is still connected
				heartbeat = null;
				nextHeartbeatTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
				heartbeatState = HeartbeatState.Ok;
				return true;
			}

			for (int i = 0; i < Configuration.JunkHashes.Count; i++) {
				var junkBytes = Configuration.JunkHashes[i];
				var heartbeatBytes = Encoding.ASCII.GetBytes(heartbeat);
				var expectedHeartbeat = HashBytes(junkBytes, heartbeatBytes);

				if (expectedHeartbeat == responseHash) {
#if DEBUG
					Log.Info($"Heartbeat: {expectedHeartbeat} == {responseHash}");
#endif

					heartbeat = null;
					nextHeartbeatTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
					heartbeatState = HeartbeatState.Ok;
					return true;
				}

				Log.Error($"Heartbeat: {expectedHeartbeat} != {responseHash}");
			}

			heartbeat = null;
			heartbeatState = HeartbeatState.Failed;
			return false;
		}

		public virtual void SendError(string message = "An error occured that forced UberStrike to halt.") {
			HasError = true;
		}

		private static string HashBytes(byte[] a, byte[] b) {
			var buffer = new byte[a.Length + b.Length];
			Buffer.BlockCopy(a, 0, buffer, 0, a.Length);
			Buffer.BlockCopy(b, 0, buffer, a.Length, b.Length);

			byte[] hash = null;
			using (var sha256 = SHA256.Create()) {
				hash = sha256.ComputeHash(buffer);
			}

			return BytesToHexString(hash);
		}

		private static string BytesToHexString(byte[] bytes) {
			var builder = new StringBuilder(64);
			for (int i = 0; i < bytes.Length; i++) {
				builder.Append(bytes[i].ToString("x2"));
			}

			return builder.ToString();
		}

		private static string GenerateHeartbeat() {
			var random = new Random((int)DateTime.UtcNow.Ticks);
			var buffer = new byte[32];
			random.NextBytes(buffer);

			return BytesToHexString(buffer);
		}
	}
}
