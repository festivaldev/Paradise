using System;

namespace Paradise.Realtime.Server.Game {
	public enum GameStateId {
		None,
		WaitingForPlayers,
		Countdown,
		MatchRunning,
		EndOfMatch,
		Debug
	}

	public abstract class GameState : IState {
		protected BaseGameRoom Room { get; private set; }

		public GameState(BaseGameRoom room) {
			Room = room ?? throw new ArgumentNullException(nameof(room));
		}

		public abstract void OnEnter();
		public abstract void OnExit();
		public abstract void OnResume();
		public abstract void OnUpdate();
	}
}