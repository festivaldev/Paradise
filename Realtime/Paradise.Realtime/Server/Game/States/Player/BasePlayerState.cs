using log4net;
using System;

namespace Paradise.Realtime.Server.Game {
	public enum PlayerStateId {
		None,
		Playing,
		Spectating,
		Killed,
		Paused,
		PrepareForMatch,
		Overview,
		AfterRound
	}

	public abstract class BasePlayerState : IState {
		private static readonly ILog Log = LogManager.GetLogger("GameLog");

		protected GamePeer Peer { get; private set; }
		protected BaseGameRoom Room => Peer.Room;

		public BasePlayerState(GamePeer peer) {
			Peer = peer ?? throw new ArgumentNullException(nameof(peer));
		}

		public abstract void OnEnter();
		public abstract void OnExit();
		public abstract void OnResume();
		public abstract void OnUpdate();
	}
}