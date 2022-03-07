using log4net;
using Paradise.Core.ViewModel;
using Paradise.WebServices.Client;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Paradise.Realtime.Server {
	public abstract class Peer : ClientPeer {
		private static readonly Random _random = new Random((int)DateTime.Now.Ticks);

		private enum HeartbeatState {
			Ok,
			Waiting,
			Failed
		}

		private string _heartbeat;
		private DateTime _heartbeatNextTime;
		private DateTime _heartbeatExpireTime;
		private HeartbeatState _heartbeatState;

		protected ILog Log { get; }

		protected UberstrikeUserViewModel UserView { get; set; }

		public int HeartbeatTimeout { get; set; }
		public int HeartbeatInterval { get; set; }

		public bool HasError { get; protected set; }
		public string AuthToken { get; protected set; }
		public OperationHandlerCollection Handlers { get; }

		protected PeerConfiguration Configuration { get; }

		public Peer(InitRequest initRequest) : base(initRequest) {
			if (!(initRequest.UserData is PeerConfiguration config))
				throw new ArgumentException("initRequest.UserData is not an instance of PeerConfiguration", nameof(initRequest));

			if (initRequest.ApplicationId != ApiVersion.Current)
				throw new ArgumentException("InitRequest had an invalid application ID", nameof(initRequest));

			Configuration = config;
			HeartbeatTimeout = config.HeartbeatTimeout;
			HeartbeatInterval = config.HeartbeatInterval;

			Log = LogManager.GetLogger(GetType().Name);
			Handlers = new OperationHandlerCollection();

			if (Configuration.JunkHashes.Count > 0)
				_heartbeatNextTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
		}

		public virtual void Tick() {
			switch (_heartbeatState) {
				case HeartbeatState.Ok:
					if (Configuration.JunkHashes.Count > 0 && DateTime.UtcNow >= _heartbeatNextTime)
						Heartbeat();
					break;
				case HeartbeatState.Waiting:
					Debug.Assert(Configuration.JunkHashes.Count > 0);
					if (DateTime.UtcNow >= _heartbeatExpireTime)
						Disconnect();
					break;
				case HeartbeatState.Failed:
					Debug.Assert(Configuration.JunkHashes.Count > 0);
					SendError();
					break;
			}
		}

		public bool Authenticate(string authToken, string magicHash) {
			AuthToken = authToken ?? throw new ArgumentNullException(nameof(authToken));

			if (magicHash == null)
				throw new ArgumentNullException(nameof(magicHash));

			Log.Info($"Authenticating {authToken}:{magicHash} at {RemoteIP}:{RemotePort}");

			var userView = GetUser(true);
			OnAuthenticate(userView);

			return true;
		}

		public void Heartbeat() {
			if (Configuration.JunkHashes.Count == 0)
				return;

			_heartbeat = GenerateHeartbeat();
			_heartbeatExpireTime = DateTime.UtcNow.AddSeconds(HeartbeatTimeout);
			_heartbeatState = HeartbeatState.Waiting;

			SendHeartbeat(_heartbeat);
		}

		public bool HeartbeatCheck(string responseHash) {
			if (responseHash == null)
				throw new ArgumentNullException(nameof(responseHash));

			if (_heartbeat == null)
				Log.Error("Heartbeat was null while checking.");
			return false;

			for (int i = 0; i < Configuration.JunkHashes.Count; i++) {
				var junkBytes = Configuration.JunkHashes[i];
				var heartbeatBytes = Encoding.ASCII.GetBytes(_heartbeat);
				var expectedHeartbeat = HashBytes(junkBytes, heartbeatBytes);

				if (expectedHeartbeat == responseHash) {
					_heartbeat = null;
					_heartbeatNextTime = DateTime.UtcNow.AddSeconds(HeartbeatInterval);
					_heartbeatState = HeartbeatState.Ok;
					return true;
				}

				Log.Error($"Heartbeat: {expectedHeartbeat} != {responseHash}");
			}

			_heartbeat = null;
			_heartbeatState = HeartbeatState.Failed;
			return false;
		}

		public abstract void SendHeartbeat(string hash);

		public virtual void SendError(string message = "An error occured that forced UberStrike to halt.") {
			HasError = true;
		}

		protected virtual void OnAuthenticate(UberstrikeUserViewModel userView) { }

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail) {
			foreach (var handler in Handlers) {
				try {
					handler.OnDisconnect(this, reasonCode, reasonDetail);
				} catch (Exception ex) {
					Log.Error($"Error while handling disconnection of peer on {handler.GetType().Name}", ex);
				}
			}
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters) {
			/* 
                OperationRequest should contain 1 value in its parameter dictionary.
                [0] -> (Int32: Channel.Id) ->> (Byte[]: RequestData).

                Check if we've got enough parameters.
             */
			if (operationRequest.Parameters.Count < 1) {
				Log.Warn($"Client at {RemoteIPAddress}:{RemotePort} did not send enough parameters. Disconnecting.");

				/* Assume protocol compliance failure, therefore disconnect. */
				Disconnect();
				return;
			}

			byte handlerId = operationRequest.Parameters.Keys.First();
			var handler = Handlers[handlerId];
			if (handler == null) {
				Log.Warn($"Client {RemoteIPAddress}:{RemotePort} sent an operation request on a handler which is not registered.");
				return;
			}

			if (!(operationRequest.Parameters[handlerId] is byte[] data)) {
				Log.Warn($"Client {RemoteIPAddress} sent an operation request but the data type was not byte[]. Disconnecting.");

				/* Assume protocol compliance failure, therefore disconnect. */
				Disconnect();
				return;
			}

			using (var bytes = new MemoryStream(data)) {
				try {
					handler.OnOperationRequest(this, operationRequest.OperationCode, bytes);
				} catch (Exception ex) {
					Log.Error($"Error while handling request on {handler.GetType().Name} -> :{operationRequest.OperationCode}", ex);
				}
			}
		}

		public UberstrikeUserViewModel GetUser(bool retrieve) {
			if (retrieve || UserView == null) {
				/* Retrieve user data from the web server. */
				Log.Debug($"Retrieving User from {Configuration.ServiceBaseURL}");
				UserView = new UserWebServiceClient(Configuration.ServiceBaseURL).GetMember(AuthToken);
	}

			return UserView;
		}

		private static string HashBytes(byte[] a, byte[] b) {
			var buffer = new byte[a.Length + b.Length];
			Buffer.BlockCopy(a, 0, buffer, 0, a.Length);
			Buffer.BlockCopy(b, 0, buffer, a.Length, b.Length);

			byte[] hash = null;
			using (var sha256 = SHA256.Create())
				hash = sha256.ComputeHash(buffer);

			return BytesToHexString(hash);
		}

		private static string BytesToHexString(byte[] bytes) {
			var builder = new StringBuilder(64);
			for (int i = 0; i < bytes.Length; i++)
				builder.Append(bytes[i].ToString("x2"));
			return builder.ToString();
		}

		private static string GenerateHeartbeat() {
			var buffer = new byte[32];
			_random.NextBytes(buffer);
			return BytesToHexString(buffer);
		}
	}
}
