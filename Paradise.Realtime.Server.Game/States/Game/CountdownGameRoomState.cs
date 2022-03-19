using log4net;

namespace Paradise.Realtime.Server.Game {
	public class CountdownGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(CountdownGameRoomState));

		public CountdownGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() { }

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
