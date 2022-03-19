using log4net;

namespace Paradise.Realtime.Server.Game {
	public class RunningGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(RunningGameRoomState));

		public RunningGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() { }

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
