using log4net;
using System;

namespace Paradise.Realtime.Server.Game {
	public enum GameStateId {
		None,
		MatchRunning,
		PregameLoadout,
		WaitingForPlayers,
		EndOfMatch,
		InGameShop,
		PrepareNextRound,
		AfterRound
	}

	public abstract class BaseMatchState : IState {
		private static readonly ILog Log = LogManager.GetLogger(nameof(BaseMatchState));

		protected BaseGameRoom Room { get; private set; }

		public BaseMatchState(BaseGameRoom room) {
			Room = room ?? throw new ArgumentNullException(nameof(room));
		}

		public abstract void OnEnter();
		public abstract void OnExit();
		public abstract void OnResume();
		public abstract void OnUpdate();
	}
}