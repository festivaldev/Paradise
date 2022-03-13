using System;

namespace Paradise.Realtime.Server.Game {
	public abstract class GamePeerState : IState {
		public enum Id {
			None,
			Overview,
			WaitingForPlayers,
			Countdown,
			Playing,
			Killed
		}

		protected GamePeer Peer { get; private set; }
		protected BaseGameRoom Room => Peer.Room;

		public GamePeerState(GamePeer peer) {
			Peer = peer ?? throw new ArgumentNullException(nameof(peer));
		}

		public abstract void OnEnter();
		public abstract void OnExit();
		public abstract void OnResume();
		public abstract void OnUpdate();
	}
}