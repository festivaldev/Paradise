using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Comm {
	public class LobbyManager {
		private static readonly ILog Log = LogManager.GetLogger(typeof(CommPeer));

		private readonly Dictionary<int, CommPeer> peers = new Dictionary<int, CommPeer>();
		private readonly object _lock = new object();

		public IEnumerable<CommPeer> Peers => peers.Values;

		public static LobbyManager Instance { get; } = new LobbyManager();

		public void Join(CommPeer peer) {
			lock (_lock) {
				peers.Add(peer.Actor.Cmid, peer);

				foreach (var otherPeer in Peers) {
					if (otherPeer.Actor.Cmid != peer.Actor.Cmid) {
						peer.LobbyEvents.SendPlayerJoined(peer.Actor.ActorInfo);
					}
				}

				Log.Info($"{peer.Actor.Name} ({peer.Actor.Cmid}) joined the lobby");

				peer.PeerEvents.SendLobbyEntered();
			}
		}

		public void Leave(CommPeer peer) {
			lock (_lock) {
				peers.Remove(peer.Actor.Cmid);

				foreach (var otherPeer in Peers) {
					peer.LobbyEvents.SendPlayerLeft(peer.Actor.Cmid, true);
				}

				Log.Info($"{peer.Actor.Name} ({peer.Actor.Cmid}) left the lobby");
			}
		}

		public void UpdatePlayerList() {
			lock (_lock) {
				var actorsInfo = new List<CommActorInfo>(peers.Count);
				foreach (var peer in Peers) {
					actorsInfo.Add(peer.Actor.ActorInfo);
				}

				foreach (var peer in Peers) {
					peer.LobbyEvents.SendFullPlayerListUpdate(actorsInfo);
				}
			}
		}
	}
}
