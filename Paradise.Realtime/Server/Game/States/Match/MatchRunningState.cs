using Paradise.Core.Models;
using Paradise.Core.Types;
using System;

namespace Paradise.Realtime.Server.Game {
	internal class MatchRunningState : BaseMatchState {
		public MatchRunningState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerLeft += OnPlayerLeft;
			Room.PlayerKilled += OnPlayerKilled;
			Room.PlayerRespawned += OnPlayerRespawned;
			Room.MatchEnded += OnMatchEnded;

			Room.RoundStartTime = Environment.TickCount;
			Room.RoundEndTime = Room.RoundStartTime + (Room.MetaData.TimeLimit * 1000);

			foreach (var player in Room.Players) {
				player.State.SetState(PlayerStateId.Playing);

				player.GameEvents.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
			}
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerLeft -= OnPlayerLeft;
			Room.PlayerKilled -= OnPlayerKilled;
			Room.PlayerRespawned -= OnPlayerRespawned;
			Room.MatchEnded -= OnMatchEnded;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			Room.PowerUpManager.Update();

			if (Environment.TickCount > Room.RoundEndTime) {
				Room.State.SetState(GameStateId.EndOfMatch);
			}
		}

		#region Handlers
		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
				args.Player.Actor.Info.PlayerState = PlayerStates.Spectator;
			} else {
				Room.PreparePlayer(args.Player);
				Room.SpawnPlayer(args.Player);

				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendPlayerJoinedGame(args.Player.Actor.Info, args.Player.Actor.Movement);
				}
			}

			args.Player.State.SetState(PlayerStateId.Playing);
			args.Player.GameEvents.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);

			if (Room.MetaData.GameMode == GameModeType.DeathMatch) {
				short killsRemaining = (short)Room.MetaData.KillLimit;

				Room.GetCurrentScore(out killsRemaining, out _, out _);
				args.Player.GameEvents.SendKillsRemaining(killsRemaining, 0);
			} else {
				short blueTeamScore = 0;
				short redTeamScore = 0;

				Room.GetCurrentScore(out _, out blueTeamScore, out redTeamScore);
				args.Player.GameEvents.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
			}
		}

		private void OnPlayerLeft(object sender, PlayerLeftEventArgs args) {
			if (Room.CanStartMatch) {
				if (Room.MetaData.GameMode == GameModeType.DeathMatch) {
					short killsRemaining = (short)Room.MetaData.KillLimit;

					Room.GetCurrentScore(out killsRemaining, out _, out _);
					foreach (var peer in Room.Peers) {
						peer.GameEvents.SendKillsRemaining(killsRemaining, 0);
					}
				}
			} else {
				Room.State.SetState(GameStateId.EndOfMatch);
			}
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs args) {
			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendPlayerKilled(args.AttackerCmid, args.VictimCmid, (byte)args.ItemClass, args.Damage, (byte)args.Part, args.Direction);

				if (peer.Actor.Cmid == args.VictimCmid) {
					peer.State.SetState(PlayerStateId.Killed);
				}
			}

			if (Room.MetaData.GameMode == GameModeType.DeathMatch) {
				short killsRemaining = (short)Room.MetaData.KillLimit;

				Room.GetCurrentScore(out killsRemaining, out _, out _);
				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendKillsRemaining(killsRemaining, 0);
				}
			} else {
				short blueTeamScore = 0;
				short redTeamScore = 0;

				Room.GetCurrentScore(out _, out blueTeamScore, out redTeamScore);
				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
				}
			}
		}

		private void OnPlayerRespawned(object sender, PlayerRespawnedEventArgs args) {
			if (Room.MetaData.GameMode == GameModeType.EliminationMode) return;

			Room.SpawnPlayer(args.Player, false);
			args.Player.State.SetState(PlayerStateId.Playing);
		}

		private void OnMatchEnded(object sender, EventArgs args) {
			Room.State.SetState(GameStateId.EndOfMatch);
		}
		#endregion
	}
}
