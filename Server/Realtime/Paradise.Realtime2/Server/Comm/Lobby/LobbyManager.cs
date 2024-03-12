using log4net;
using System.Collections.Generic;
using UberStrike.Core.Models;

namespace Paradise.Realtime.Server.Comm {
	public class LobbyManager {
		protected static readonly ILog Log = LogManager.GetLogger(nameof(LobbyManager));

		public static LobbyManager Instance { get; } = new LobbyManager();

		public LobbyRoom GlobalLobby { get; private set; } = new LobbyRoom();
		public IReadOnlyList<CommPeer> Peers => GlobalLobby.Peers;

		private readonly object _lock = new object();

		public void UpdatePlayerList() {
			lock (_lock) {
				var actorsInfo = new List<CommActorInfo>(Peers.Count);
				foreach (var peer in Peers) {
					actorsInfo.Add(peer.Actor.ActorInfo);
				}

				foreach (var peer in Peers) {
					peer.LobbyEventSender.SendFullPlayerListUpdate(actorsInfo);
				}
			}
		}
	}
}
