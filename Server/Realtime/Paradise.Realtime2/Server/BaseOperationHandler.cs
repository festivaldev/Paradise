using log4net;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime.Server {
	public enum OperationHandlerId {
		LobbyRoom = 0,
		CommPeer,
		GameRoom = 0,
		GamePeer
	}

	public abstract class BaseOperationHandler {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(BaseOperationHandler));

		public abstract int Id { get; }

		public abstract void OnOperationRequest(BasePeer peer, byte opCode, MemoryStream bytes);
		public virtual void OnDisconnect(BasePeer peer, DisconnectReason reasonCode, string reasonDetail) { }
	}

	public abstract class BaseOperationHandler<TPeer, TOperationType> : BaseOperationHandler
		where TPeer : BasePeer
		where TOperationType : Enum {
		public virtual Dictionary<TOperationType, int> RateLimiterIntervals { get; }
		public virtual Dictionary<TOperationType, int> RateLimiterThresholds { get; }

		public abstract void OnOperationRequest(TPeer peer, byte opCode, MemoryStream bytes);

		public sealed override void OnOperationRequest(BasePeer peer, byte opCode, MemoryStream bytes) {
			if (!CheckCallingRate(peer, opCode)) {
				peer.Disconnect();
			}

			OnOperationRequest((TPeer)peer, opCode, bytes);
		}

		public virtual void OnDisconnect(TPeer peer, DisconnectReason reasonCode, string reasonDetail) { }

		public sealed override void OnDisconnect(BasePeer peer, DisconnectReason reasonCode, string reasonDetail) {
			OnDisconnect((TPeer)peer, reasonCode, reasonDetail);
		}

		private bool CheckCallingRate(BasePeer peer, byte opCode) {
			var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			var opEnum = (TOperationType)Enum.ToObject(typeof(TOperationType), opCode);

			if (RateLimiterIntervals != null && RateLimiterIntervals.ContainsKey(opEnum)) {
				var rateLimit = RateLimiterIntervals[opEnum];
				var threshold = 5;

				if (RateLimiterThresholds != null && RateLimiterThresholds.ContainsKey(opEnum)) {
					threshold = RateLimiterThresholds[opEnum];
				}

				if (peer.LastOperationTime.ContainsKey(opCode)) {
					var delta = TimeSpan.FromMilliseconds(currentTimestamp - peer.LastOperationTime[opCode]).TotalMilliseconds;

					if (delta < rateLimit) {
						if (!peer.OperationSpamCounter.ContainsKey(opCode)) {
							peer.OperationSpamCounter[opCode] = 0;
						}

						peer.OperationSpamCounter[opCode]++;

						if (peer.OperationSpamCounter[opCode] >= threshold) {
							Log.Info($"{peer} exceeded RateLimit @ {opEnum} ({rateLimit}ms x {threshold})");

							return false;
						}
					} else {
						if (peer.OperationSpamCounter.ContainsKey(opCode) && peer.OperationSpamCounter[opCode] > 0) {
							peer.OperationSpamCounter[opCode]--;
						}
					}
				}

				peer.LastOperationTime[opCode] = currentTimestamp;
			}

			return true;
		}
	}
}
