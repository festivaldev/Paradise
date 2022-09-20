using log4net;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Comm {
	public partial class LobbyRoom : BaseLobbyRoomOperationHandler, IRoom<CommPeer>, IDisposable {
		private static readonly ILog Log = LogManager.GetLogger(nameof(LobbyRoom));

		private bool IsDisposed = false;

		private List<CommPeer> _peers = new List<CommPeer>();
		public IReadOnlyList<CommPeer> Peers => _peers.AsReadOnly();

		public Loop Loop { get; private set; }
		public LoopScheduler Scheduler { get; private set; }

		public LobbyRoom() {
			Loop = new Loop(OnTick, OnTickError);

			Scheduler = new LoopScheduler(5);
			Scheduler.Schedule(Loop);
			Scheduler.Start();
		}

		public void Join(CommPeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			lock (_peers) {
				if (_peers.Find(_ => _.PeerId.CompareTo(peer.PeerId) == 0) != null) {
					_peers.RemoveAll(_ => _.PeerId.CompareTo(peer.PeerId) == 0);
				}

				_peers.Add(peer);
			}

			peer.Lobby = this;
			peer.AddOperationHandler(this);

			peer.PeerEvents.SendLobbyEntered();

			Log.Info($"{peer.Actor.Name}({peer.Actor.Cmid}, {peer.PeerId}) joined the lobby");
		}

		public void Leave(CommPeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			foreach (var otherPeer in Peers) {
				otherPeer.LobbyEvents.SendPlayerLeft(peer.Actor.Cmid, true);
			}

			lock (_peers) {
				_peers.Remove(peer);
			}

			Log.Info($"{peer.Actor.Name}({peer.Actor.Cmid}, {peer.PeerId}) left the lobby");

			peer.RemoveOperationHandler(Id);
			peer.Actor = null;
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
			Log.Info("loop tick error", e);
		}

		public void Dispose() {
			Dispose(true);
		}

		private void Dispose(bool disposing) {
			if (IsDisposed) return;

			if (disposing) {
				Scheduler.Unschedule(Loop);

				foreach (var peer in Peers) {
					foreach (var otherPeer in Peers) {
						if (peer == otherPeer) continue;

						peer.LobbyEvents.SendPlayerLeft(peer.Actor.Cmid, true);
					}
				}

				foreach (var peer in Peers) {
					peer.Actor = null;
					peer.RemoveOperationHandler(Id);

					peer.Disconnect();
					peer.Dispose();
				}

				_peers.Clear();
			}

			IsDisposed = true;
		}
	}
}
