using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public class TeamEliminationGameRoom : BaseGameRoom {
		private static readonly ILog Log = LogManager.GetLogger(typeof(TeamEliminationGameRoom));

		public TeamEliminationGameRoom(GameRoomData data, ILoopScheduler scheduler) : base(data, scheduler) {

		}
	}
}
