using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	internal class TeamDeathMatchGameRoom : BaseGameRoom {
		private readonly static ILog Log = LogManager.GetLogger(nameof(TeamDeathMatchGameRoom));

		private Dictionary<TeamID, int> TeamScores;

		public TeamDeathMatchGameRoom(GameRoomData metaData, ILoopScheduler scheduler) : base(metaData, scheduler) {
			TeamScores = new Dictionary<TeamID, int> {
				[TeamID.BLUE] = 0,
				[TeamID.RED] = 0
			};
		}

		public override bool CanJoinMatch => true;
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

		protected override void OnMatchEnded(EventArgs args) {
			WinningTeam = TeamScores.OrderByDescending(_ => _.Value).First().Key;

			base.OnMatchEnded(args);
		}

		protected override void OnPlayerKilled(PlayerKilledEventArgs args) {
			foreach (var player in Players) {
				if (player.Actor.Cmid != args.AttackerCmid) {
					continue;
				}

				if (args.AttackerCmid == args.VictimCmid) {
					//TeamScores[player.Actor.Team] -= 1;
					//break;
				} else {
					TeamScores[player.Actor.Team] += 1;
					break;
				}
			}

			base.OnPlayerKilled(args);

			foreach (var kvp in TeamScores) {
				if (kvp.Value >= MetaData.KillLimit) {
					HasRoundEnded = true;
					break;
				}
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


			foreach (var otherPeer in Peers) {
				otherPeer.GameEvents.SendPlayerKilled(peer.Actor.Cmid, peer.Actor.Cmid, (byte)UberstrikeItemClass.WeaponMachinegun, 0, (byte)BodyPart.Body, Vector3.zero);
			}

			peer.State.SetState(PlayerStateId.Killed);
			peer.PreviousSpawnPoints.Clear();
			peer.Actor.LastTeamSwitchTime = DateTime.UtcNow;

			foreach (var otherPeer in Peers) {
				otherPeer.GameEvents.SendPlayerChangedTeam(peer.Actor.Cmid, peer.Actor.Team);
			}
		}
	}
}
