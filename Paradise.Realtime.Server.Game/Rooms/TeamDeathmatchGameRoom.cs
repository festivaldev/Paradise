using log4net;
using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	public class TeamDeathmatchGameRoom : BaseGameRoom {
		private static readonly ILog Log = LogManager.GetLogger(typeof(TeamDeathmatchGameRoom));

		public TeamDeathmatchGameRoom(GameRoomData data, ILoopScheduler scheduler) : base(data, scheduler) {

		}
	}
}
