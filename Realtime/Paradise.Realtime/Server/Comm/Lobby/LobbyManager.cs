using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Comm {
	public class LobbyManager {
		private static readonly ILog Log = LogManager.GetLogger(nameof(LobbyManager));

		private readonly object _lock = new object();


		public LobbyRoom GlobalLobby { get; private set; } = new LobbyRoom();
		public IReadOnlyList<CommPeer> Peers => GlobalLobby.Peers;

		public static LobbyManager Instance { get; } = new LobbyManager();

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
