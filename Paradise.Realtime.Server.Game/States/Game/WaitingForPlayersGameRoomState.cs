using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class WaitingForPlayersGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(WaitingForPlayersGameRoomState));

		public WaitingForPlayersGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() { }

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
