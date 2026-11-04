/*
 * Copyright (C) 2017, 2022-2026 Team FESTIVAL
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using UberStrike.Core.Models;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using static Paradise.Realtime.Server.Game.BaseGameRoom;

namespace Paradise.Realtime.Server.Game {
	internal class MatchRunningState : BaseMatchState {
		public MatchRunningState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.MatchEnded += OnMatchEnded;
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerLeft += OnPlayerLeft;
			Room.PlayerKilled += OnPlayerKilled;
			Room.PlayerRespawned += OnPlayerRespawned;

			Room.RoundStartTime = SystemTime.Running;

			if (Room.MetaData.TimeLimit > 0) {
				Room.RoundEndTime = Room.RoundStartTime + (Room.MetaData.TimeLimit * 1000);
			} else {
				Room.RoundEndTime = Room.RoundStartTime;
			}

			foreach (var player in Room.Players) {
				player.GameEventSender.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
				player.State.SetState(PlayerStateId.Playing);
			}

			if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
				Room.GetCurrentScore(out _, out short blueTeamScore, out short redTeamScore);
				foreach (var peer in Room.Peers) {
					peer.GameEventSender.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
				}
			}
		}

		public override void OnExit() {
			Room.MatchEnded -= OnMatchEnded;
			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerLeft -= OnPlayerLeft;
			Room.PlayerKilled -= OnPlayerKilled;
			Room.PlayerRespawned -= OnPlayerRespawned;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			Room.PowerUpManager.Update();

			if (Room.MetaData.TimeLimit > 0 && Room.RoundEndTime > 0 && SystemTime.Running > Room.RoundEndTime) {
				Room.HasRoundEnded = true;
			}
		}



		private void OnMatchEnded(object sender, EventArgs args) {
			Room.State.SetState(GameStateId.EndOfMatch);
		}

		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			args.Player.Actor.PreviousSpawnPoints.Clear();

			if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
				args.Player.Actor.ActorInfo.PlayerState = PlayerStates.Spectator;
			}

			Room.PreparePlayer(args.Player, args.Player.Actor.ActorInfo.IsSpectator);

			if (args.Player.Actor.ActorInfo.IsSpectator) {
				args.Player.State.SetState(PlayerStateId.Spectating);
				return;
			}

			Room.SpawnPlayer(args.Player, true);

			args.Player.GameEventSender.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
			//args.Player.GameEvents.SendWaitingForPlayers();

			foreach (var peer in Room.Peers) {
				peer.GameEventSender.SendPlayerRespawned(args.Player.Actor.Cmid, args.Player.Actor.Movement.Position, args.Player.Actor.Movement.HorizontalRotation);
			}

			args.Player.State.SetState(PlayerStateId.Playing);

			if (Room.MetaData.GameMode == GameModeType.DeathMatch) {
				short killsRemaining = (short)Room.MetaData.KillLimit;

				Room.GetCurrentScore(out killsRemaining, out _, out _);

				args.Player.GameEventSender.SendKillsRemaining(killsRemaining, 0);
			} else {
				Room.GetCurrentScore(out _, out short blueTeamScore, out short redTeamScore);

				args.Player.GameEventSender.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
			}
		}

		private void OnPlayerLeft(object sender, PlayerLeftEventArgs args) {
			if (TryEndMatch(Room.MetaData.GameMode == GameModeType.EliminationMode)) {
				return;
			}

			if (!Room.CanStartMatch || Room.Players.Count == 0) {
				Room.HasRoundEnded = true;
			}
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs args) {
			foreach (var peer in Room.Peers) {
				peer.GameEventSender.SendPlayerKilled(args.AttackerCmid, args.VictimCmid, (byte)args.ItemClass, args.Damage, (byte)args.Part, args.Direction);

				if (peer.Actor.Cmid.CompareTo(args.VictimCmid) == 0) {
					peer.State.SetState(PlayerStateId.Killed);
				}
			}

			switch (Room.MetaData.GameMode) {
				case GameModeType.DeathMatch: {
					var killsRemaining = (short)Room.MetaData.KillLimit;

					Room.GetCurrentScore(out killsRemaining, out _, out _);
					foreach (var peer in Room.Peers) {
						peer.GameEventSender.SendKillsRemaining(killsRemaining, 0);
					}

					break;
				}

				case GameModeType.TeamDeathMatch:
					Room.GetCurrentScore(out _, out short blueTeamScore, out short redTeamScore);
					foreach (var peer in Room.Peers) {
						peer.GameEventSender.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
					}
					break;

				case GameModeType.EliminationMode:
					if (TryEndMatch()) return;

					break;

				default: break;
			}
		}

		private void OnPlayerRespawned(object sender, PlayerRespawnedEventArgs args) {
			if (Room.MetaData.GameMode == GameModeType.EliminationMode) return;

			Room.SpawnPlayer(args.Player, false);

			args.Player.State.ResetState();
			args.Player.State.SetState(PlayerStateId.Playing);
		}

		private bool TryEndMatch(bool excludeKilledPlayers = true) {
			var remainingPlayers = Room.Players
				.Where(p => !p.Actor.ActorInfo.IsSpectator && (!excludeKilledPlayers || p.State.CurrentStateId != PlayerStateId.Killed))
				.ToList();

			if (remainingPlayers.Count == 1) {
				var winner = remainingPlayers[0];

				Room.WinningCmid = winner.Actor.Cmid;
				Room.WinningTeam = winner.Actor.Team;
				Room.HasRoundEnded = true;

				return true;
			} else if (remainingPlayers.Count == 0) {
				Room.WinningCmid = 0;
				Room.WinningTeam = TeamID.NONE;
				Room.HasRoundEnded = true;

				return true;
			}

			return false;
		}
	}
}
