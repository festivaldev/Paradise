﻿using log4net;
using Paradise.Core.Models;
using System;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class TestGameRoom : BaseGameRoom {
		private readonly static ILog Log = LogManager.GetLogger(nameof(TestGameRoom));

		public TestGameRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) {

		}

		public override bool CanStartMatch => Players.Count > 1;

		protected override void OnPlayerKilled(PlayerKilledEventArgs args) {
			base.OnPlayerKilled(args);

			var leader = Players.OrderByDescending(_ => _.Actor.Info.Kills).First();

			foreach (var peer in Peers) {
				peer.GameEvents.SendKillsRemaining(MetaData.KillLimit - leader.Actor.Info.Kills, leader.Actor.Cmid);
			}

			if (leader.Actor.Info.Kills >= MetaData.KillLimit) {
				OnMatchEnded(new EventArgs());
			}
		}
	}
}
