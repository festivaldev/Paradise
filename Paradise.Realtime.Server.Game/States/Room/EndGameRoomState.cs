using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	public class EndGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(EndGameRoomState));

		public EndGameRoomState(BaseGameRoom room) : base(room) {

		}

		public override void OnEnter() { }

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() { }
	}
}
