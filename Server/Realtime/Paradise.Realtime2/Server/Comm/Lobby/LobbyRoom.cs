using log4net;
using Paradise.Realtime.Core;
using System;
using System.Collections.Generic;
using static Paradise.WebSocket;

namespace Paradise.Realtime.Server.Comm {
	public partial class LobbyRoom : IRoom<CommPeer>, IDisposable {
		public const int TICK_RATE = 20;

		protected static readonly ILog Log = LogManager.GetLogger(typeof(LobbyRoom));

		private readonly LobbyRoom.OperationHandler OpHandler = new OperationHandler();

		private bool IsDisposed = false;

		private readonly List<CommPeer> peers = new List<CommPeer>();
		public IReadOnlyList<CommPeer> Peers => peers.AsReadOnly();

		public Loop Loop { get; private set; }
		public LoopScheduler Scheduler { get; private set; } = new LoopScheduler(TICK_RATE);

		public LobbyRoom() {
			Loop = new Loop(OnTick, OnTickError);
			Scheduler.Schedule(Loop);
			Scheduler.Start();
		}

		public void Join(CommPeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			lock (peers) {
				if (peers.Find(_ => _.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0) != null) {
					peers.RemoveAll(_ => _.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0);
				}

				peers.Add(peer);
			}

			peer.Lobby = this;
			peer.AddOperationHandler(OpHandler);

			peer.PeerEventSender.SendLobbyEntered();

			Log.Info($"{peer} joined the lobby");

			if (CommServerApplication.Instance.Configuration.DiscordPlayerAnnouncements) {
				CommServerApplication.Instance.SocketClient?.SendSync(PacketType.PlayerJoined, peer.Actor.ActorInfo, serverType: ServerType.Comm);
			}
		}

		public void Leave(CommPeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			foreach (var otherPeer in Peers) {
				otherPeer.LobbyEventSender.SendPlayerLeft(peer.Actor.Cmid, true);
			}

			lock (peers) {
				peers.Remove(peer);
			}

			Log.Info($"{peer} left the lobby");

			if (CommServerApplication.Instance.Configuration.DiscordPlayerAnnouncements) {
				CommServerApplication.Instance.SocketClient?.SendSync(PacketType.PlayerLeft, peer.Actor.ActorInfo, serverType: ServerType.Comm);
			}

			peer.RemoveOperationHandler(OpHandler.Id);
			peer.Lobby = null;
		}

		private void OnTick() {
			foreach (var peer in Peers) {
				if (peer.HasError) {
					peer.Disconnect();

					continue;
				}

				peer.Tick();
			}
		}

		private void OnTickError(Exception e) {
			Log.Error("loop tick error", e);
		}

		public void Dispose() {
			Dispose(true);
		}

		private void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				Scheduler.Unschedule(Loop);

				foreach (var peer in Peers) {
					foreach (var otherPeer in Peers) {
						if (peer == otherPeer)
							continue;

						peer.LobbyEventSender.SendPlayerLeft(peer.Actor.Cmid, true);
					}
				}

				foreach (var peer in Peers) {
					peer.Actor = null;
					peer.RemoveOperationHandler(OpHandler.Id);

					peer.Disconnect();
					peer.Dispose();
				}

				peers.Clear();
			}

			IsDisposed = true;
		}
	}
}
