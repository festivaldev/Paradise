using log4net;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime.Server {
	public abstract class BaseEventSender {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(BaseEventSender));

		public BasePeer Peer { get; private set; }

		protected BaseEventSender(BasePeer peer) {
			Peer = peer ?? throw new ArgumentNullException(nameof(peer));
		}

		protected SendResult SendEvent(byte opCode, MemoryStream bytes, bool unreliable) {
			var eventData = new EventData(opCode, new Dictionary<byte, object> {
				{ 0, bytes.ToArray() }
			});

			var result = Peer.SendEvent(eventData, new SendParameters { Unreliable = unreliable });
			if (result != SendResult.Ok && result != SendResult.Disconnected) {
				Log.Error($"Send event failed {opCode} -> {result}");
			}

			return result;
		}

		public virtual SendResult SendEvent(byte opCode, MemoryStream bytes) => SendEvent(opCode, bytes, false);
	}
}
