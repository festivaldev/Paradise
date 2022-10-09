﻿using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public class TestGameRoom : BaseGameRoom {
		private readonly static ILog Log = LogManager.GetLogger(nameof(TestGameRoom));

		public TestGameRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) { }

		public override bool CanJoinMatch => true;
		//public override bool CanJoinMatch => State.CurrentStateId != GameStateId.MatchRunning;
		public override bool CanStartMatch => Players.Count > 1;

		public override void GetCurrentScore(out short killsRemaining, out short blueTeamScore, out short redTeamScore) {
			var leader = Players.OrderByDescending(_ => _.Actor.Info.Kills).First();

			killsRemaining = (short)(MetaData.KillLimit - leader.Actor.Info.Kills);
			blueTeamScore = 1;
			redTeamScore = 3;
		}

		protected override void OnMatchEnded(EventArgs args) {
			WinningTeam = TeamID.BLUE;

			base.OnMatchEnded(args);
		}

		protected override void OnPlayerKilled(PlayerKilledEventArgs args) {
			base.OnPlayerKilled(args);

			var leader = Players.OrderByDescending(_ => _.Actor.Info.Kills).First();
			if (leader.Actor.Info.Kills >= MetaData.KillLimit) {
				WinningCmid = leader.Actor.Cmid;

				HasRoundEnded = true;
			}
		}

		protected override void OnSwitchTeam(GamePeer peer) {
			if (State.CurrentStateId == GameStateId.PrepareNextRound) return;
			if (Math.Abs((peer.Actor.LastTeamSwitchTime - DateTime.UtcNow).TotalSeconds) < 30) return;

			if (peer.Actor.Team == TeamID.BLUE) {
				peer.Actor.Team = TeamID.RED;
			} else if (peer.Actor.Team == TeamID.RED) {
				peer.Actor.Team = TeamID.BLUE;
			}

			peer.Actor.Info.PlayerState |= PlayerStates.Dead;

			OnPlayerKilled(new PlayerKilledEventArgs {
				AttackerCmid = peer.Actor.Cmid,
				VictimCmid = peer.Actor.Cmid,
				ItemClass = UberstrikeItemClass.WeaponMachinegun,
				Part = BodyPart.Body,
				Direction = new Vector3()
			});

			peer.Actor.LastTeamSwitchTime = DateTime.UtcNow;

			foreach (var otherPeer in Peers) {
				otherPeer.GameEvents.SendPlayerChangedTeam(peer.Actor.Cmid, peer.Actor.Team);
			}
		}
	}
}
