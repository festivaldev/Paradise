using log4net;

namespace Paradise.Realtime.Server.Game {
	public class EndGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(EndGameRoomState));

		public EndGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() { }

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
