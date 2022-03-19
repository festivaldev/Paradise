using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	public class TestGameRoom : BaseGameRoom {
		public TestGameRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) {

		}
	}
}
