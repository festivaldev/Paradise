using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	internal class TeamEliminationRoom : BaseGameRoom {
		private static readonly ILog Log = LogManager.GetLogger(nameof(TeamEliminationRoom));

		private Dictionary<TeamID, int> TeamScores;

		public TeamEliminationRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) {
			TeamScores = new Dictionary<TeamID, int> {
				[TeamID.BLUE] = 0,
				[TeamID.RED] = 0
			};
		}

		public override bool CanJoinMatch => State.CurrentStateId != GameStateId.MatchRunning;
		public override bool CanStartMatch => Players.Where(_ => _.Actor.Team == TeamID.BLUE).Count() >= 1 &&
											  Players.Where(_ => _.Actor.Team == TeamID.RED).Count() >= 1;

		public override void GetCurrentScore(out short killsRemaining, out short blueTeamScore, out short redTeamScore) {
			killsRemaining = 0;
			blueTeamScore = (short)TeamScores[TeamID.BLUE];
			redTeamScore = (short)TeamScores[TeamID.RED];
		}

		public override void Reset() {
			TeamScores[TeamID.BLUE] = 0;
			TeamScores[TeamID.RED] = 0;

			base.Reset();
		}

		protected override void OnPlayerKilled(PlayerKilledEventArgs args) {
			if (Players.Where(_ => _.Actor.Team == TeamID.RED && _.Actor.ActorInfo.IsAlive && !_.Actor.ActorInfo.IsSpectator).Count() == 0) {
				TeamScores[TeamID.BLUE] += 1;
				WinningTeam = TeamID.BLUE;

				HasRoundEnded = true;
			} else if (Players.Where(_ => _.Actor.Team == TeamID.BLUE && _.Actor.ActorInfo.IsAlive && !_.Actor.ActorInfo.IsSpectator).Count() == 0) {
				TeamScores[TeamID.RED] += 1;
				WinningTeam = TeamID.RED;

				HasRoundEnded = true;
			}

			base.OnPlayerKilled(args);
		}

		protected override void SwitchTeam(GamePeer peer) {
			if (State.CurrentStateId == GameStateId.PrepareNextRound) return;
			if (Peers.Where(_ => _.Actor.Team != peer.Actor.Team).Count() >= Peers.Where(_ => _.Actor.Team == peer.Actor.Team).Count()) return;
			if (Math.Abs((peer.Actor.LastTeamSwitchTime - DateTime.UtcNow).TotalSeconds) < 5) return;

			if (peer.Actor.Team == TeamID.BLUE) {
				peer.Actor.Team = TeamID.RED;
			} else if (peer.Actor.Team == TeamID.RED) {
				peer.Actor.Team = TeamID.BLUE;
			}

			peer.Actor.ActorInfo.PlayerState |= PlayerStates.Dead;


			foreach (var otherPeer in Peers) {
				otherPeer.GameEventSender.SendPlayerKilled(peer.Actor.Cmid, peer.Actor.Cmid, (byte)UberstrikeItemClass.WeaponMachinegun, 0, (byte)BodyPart.Body, Vector3.zero);
			}

			peer.State.SetState(PlayerStateId.Killed);
			peer.Actor.PreviousSpawnPoints.Clear();
			peer.Actor.LastTeamSwitchTime = DateTime.UtcNow;

			foreach (var otherPeer in Peers) {
				otherPeer.GameEventSender.SendPlayerChangedTeam(peer.Actor.Cmid, peer.Actor.Team);
			}
		}
	}
}
