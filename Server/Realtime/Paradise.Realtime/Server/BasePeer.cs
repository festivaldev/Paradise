using Cmune.DataCenter.Common.Entities;
using log4net;
using Newtonsoft.Json;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UberStrike.Core.ViewModel;
using UberStrike.Realtime.UnitySdk;

namespace Paradise.Realtime.Server {
	public enum HeartbeatState {
		Ok,
		Waiting,
		Failed
	}

	[JsonObject(MemberSerialization.OptIn)]
	public abstract class BasePeer : ClientPeer {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(BasePeer));

		private readonly ConcurrentDictionary<int, BaseOperationHandler> OperationHandlers;

		public bool HasError { get; protected set; }
		public string AuthToken { get; protected set; }

		public UberstrikeUserViewModel Member { get; protected set; }

		protected PeerConfiguration Configuration { get; }
		public int HeartbeatInterval { get; set; }
		public int HeartbeatTimeout { get; set; }

		private string heartbeat;
		private DateTime nextHeartbeatTime;
		private DateTime heartbeatExpireTime;
		private HeartbeatState heartbeatState;

		public readonly Dictionary<int, long> LastOperationTime = new Dictionary<int, long>();
		public readonly Dictionary<int, int> OperationSpamCounter = new Dictionary<int, int>();

		public BasePeer(InitRequest initRequest) : base(initRequest) {
			if (initRequest.ApplicationId != ApiVersion.Current) {
				Disconnect();
				return;
			}

			OperationHandlers = new ConcurrentDictionary<int, BaseOperationHandler>();

			if (initRequest.UserData is PeerConfiguration config) {
				Configuration = config;
				HeartbeatInterval = config.HeartbeatInterval;
				HeartbeatTimeout = config.HeartbeatTimeout;
			}

			if (Configuration.JunkHashes.Count > 0) {
				nextHeartbeatTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
			}
		}

		public void AddOperationHandler(BaseOperationHandler handler) {
			if (handler == null) {
				throw new ArgumentNullException(nameof(handler));
			}

			if (OperationHandlers.ContainsKey(handler.Id)) {
				OperationHandlers.TryRemove(handler.Id, out _);
			}

			if (!OperationHandlers.TryAdd(handler.Id, handler)) {
				Log.Error($"Failed to add handler with ID {handler.Id}");
			}
		}

		public void RemoveOperationHandler(int handlerId) {
			if (!OperationHandlers.TryRemove(handlerId, out _)) {
				Log.Error($"Failed to remove handler with ID {handlerId}");
			}
		}

		public bool Authenticate(string authToken, string magicHash) {
			AuthToken = authToken ?? throw new ArgumentNullException(nameof(authToken));

			var memberAuth = AuthenticationWebServiceClient.Instance.VerifyAuthToken(authToken);

			if (memberAuth.MemberAuthenticationResult != MemberAuthenticationResult.Ok) {
				return false;
			}

			if (magicHash == null) {
				throw new ArgumentNullException(nameof(magicHash));
			}

			Log.Debug($"Received AuthenticationRequest! {authToken}:{magicHash} (at {RemoteIP}:{RemotePort})");

			if (!Configuration.HashVerificationEnabled) return true;

			if (Configuration.CompositeHashes.Count > 0) {
				var bytes = Encoding.ASCII.GetBytes(authToken);

				foreach (var hash in Configuration.CompositeHashes) {
					var text = HashBytes(hash, bytes);

					if (text.Equals(magicHash)) {
						Log.Debug($"MagicHash: {text} == {magicHash}");
						return true;
					}

					Log.Debug($"MagicHash: {text} != {magicHash}");
				}

				return false;
			}

			return true;
		}

		public virtual void SendError(string message = "An error occured that forced UberStrike to halt.") {
			HasError = true;
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail) {
			foreach (var opHandler in OperationHandlers.Values) {
				try {
					opHandler.OnDisconnect(this, reasonCode, reasonDetail);
				} catch (Exception ex) {
					BaseRealtimeApplication.Instance.HandleException(ex);
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

			if (OperationHandlers.TryGetValue(handlerId, out var handler)) {
				var data = (byte[])operationRequest.Parameters[handlerId];
				using (var bytes = new MemoryStream(data)) {
					try {
						handler.OnOperationRequest(this, operationRequest.OperationCode, bytes);
					} catch (NotImplementedException ex) {
						var stackTrace = new System.Diagnostics.StackTrace(ex);
						Log.Debug($"Not Implemented: {handler.GetType().Name}:{stackTrace.GetFrame(0).GetMethod().Name}");
					} catch (NotSupportedException ex) {
						var stackTrace = new System.Diagnostics.StackTrace(ex);
						Log.Debug($"Not Supported: {handler.GetType().Name}:{stackTrace.GetFrame(0).GetMethod().Name}");
					} catch (Exception ex) {
						BaseRealtimeApplication.Instance.HandleException(ex);
						Log.Error($"Error while handling request {handler.GetType().Name}:{handlerId} -> OpCode: {operationRequest.OperationCode}", ex);
					}
				}
			} else {
				Log.Warn($"Unable to handle operation request -> operation handler not implemented: {handlerId}");
			}
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
						Log.Debug($"Disconnecting {this} because Heartbeat time expired");
						Disconnect();
					}

					break;
				case HeartbeatState.Failed:
					if (Member.CmuneMemberView.PublicProfile.AccessLevel != MemberAccessLevel.Admin) {
						Log.Debug($"Disconnecting {this} because heartbeat failed");
						SendError();
					}

					break;
				default: break;
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
//			Log.Debug($"CheckHeartbeat({responseHash})");
//#endif

			if (heartbeat == null) {
				Log.Error("Heartbeat was null while checking.");
				return false;
			}

			if (!Configuration.HashVerificationEnabled) {
				// We'll just pass through the heartbeat response for now
				// as a way to check if the peer is still connected
				heartbeat = null;
				nextHeartbeatTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
				heartbeatState = HeartbeatState.Ok;
				return true;
			}

			foreach (var hash in Configuration.JunkHashes) {
				var heartbeatBytes = Encoding.ASCII.GetBytes(heartbeat);
				var expectedHeartbeat = HashBytes(hash, heartbeatBytes);

				if (expectedHeartbeat == responseHash) {
#if DEBUG
					Log.Error($"Heartbeat: {expectedHeartbeat} == {responseHash}");
#endif

					heartbeat = null;
					heartbeatState = HeartbeatState.Failed;
					return false;
				}

#if DEBUG
				Log.Debug($"Heartbeat: {expectedHeartbeat} != {responseHash}");
#endif
			}

			heartbeat = null;
			nextHeartbeatTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
			heartbeatState = HeartbeatState.Ok;
			return true;
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
