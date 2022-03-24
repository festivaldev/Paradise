using Paradise.Core.Models;

namespace Paradise.Realtime.Server.Game {
	public class EndOfMatchGameState : GameState {
		private Countdown RestartCountdown;

		public EndOfMatchGameState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			RestartCountdown = new Countdown(Room.Loop, 6, 0);
			RestartCountdown.Completed += OnRestartCountdownCompleted;

			foreach (var peer in Room.Peers) {
				//	peer.GameEvents.SendTeamWins(TeamID.BLUE);
				//	peer.GameEvents.SendMatchEnd(new EndOfMatchData {
				//		HasWonMatch = true,
				//		MatchGuid = Room.MetaData.Guid
				//	});

				peer.GameEvents.SendTeamWins(Room.WinningTeam);
				peer.State.SetState(PlayerStateId.AfterRound);
			}
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }



		private void OnRestartCountdownCompleted() {
			Room.State.SetState(GameStateId.MatchRunning);
		}
	}
}