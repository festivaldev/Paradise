using System;

namespace Paradise.Realtime.Server.Game {
	public enum PlayerStateId {
		None,
		Overview,
		WaitingForPlayers,
		Countdown,
		Playing,
		Killed,
		AfterRound,
		Debug
	}

	public abstract class PlayerState : IState {
		protected GamePeer Peer { get; private set; }
		protected BaseGameRoom Room => Peer.Room;

		public PlayerState(GamePeer peer) {
			Peer = peer ?? throw new ArgumentNullException(nameof(peer));
		}

		public abstract void OnEnter();
		public abstract void OnExit();
		public abstract void OnResume();
		public abstract void OnUpdate();
	}
}