using System;

namespace Paradise.Realtime.Server.Game {
	public abstract class GameRoomState : IState {
		public enum Id {
			None,
			WaitingForPlayers,
			Countdown,
			Running,
			End,

			Debug
		}

		protected BaseGameRoom Room { get; private set; }

		public GameRoomState(BaseGameRoom room) {
			Room = room ?? throw new ArgumentNullException(nameof(room));
		}

		public abstract void OnEnter();
		public abstract void OnExit();
		public abstract void OnResume();
		public abstract void OnUpdate();
	}
}