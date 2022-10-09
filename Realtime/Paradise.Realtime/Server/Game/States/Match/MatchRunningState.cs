using Paradise.Core.Models;
using Paradise.Core.Types;
using System;

namespace Paradise.Realtime.Server.Game {
	internal class MatchRunningState : BaseMatchState {
		public MatchRunningState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.MatchEnded += OnMatchEnded;
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerLeft += OnPlayerLeft;
			Room.PlayerKilled += OnPlayerKilled;
			Room.PlayerRespawned += OnPlayerRespawned;

			Room.RoundStartTime = Environment.TickCount;
			Room.RoundEndTime = Room.RoundStartTime + (Room.MetaData.TimeLimit * 1000);

			foreach (var player in Room.Players) {
				player.GameEvents.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
				player.State.SetState(PlayerStateId.Playing);
			}

			if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
				short blueTeamScore = 0;
				short redTeamScore = 0;

				Room.GetCurrentScore(out _, out blueTeamScore, out redTeamScore);
				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendUpdateRoundScore(Room.RoundNumber, blueTeamScore, redTeamScore);
				}
			}
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerLeft -= OnPlayerLeft;
			Room.PlayerKilled -= OnPlayerKilled;
			Room.PlayerRespawned -= OnPlayerRespawned;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			Room.PowerUpManager.Update();

			if (Environment.TickCount > Room.RoundEndTime) {
				Room.HasRoundEnded = true;
			}
		}



		private void OnMatchEnded(object sender, EventArgs args) {
			Room.State.SetState(GameStateId.EndOfMatch);
		}

		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			args.Player.PreviousSpawnPoints.Clear();

			if (Room.MetaData.GameMode == GameModeType.EliminationMode) {
				args.Player.Actor.Info.PlayerState = PlayerStates.Spectator;
			} else {
				Room.PreparePlayer(args.Player);
				Room.SpawnPlayer(args.Player, true);

				args.Player.GameEvents.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
				args.Player.GameEvents.SendWaitingForPlayers();

				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendPlayerRespawned(args.Player.Actor.Cmid, args.Player.Actor.Movement.Position, args.Player.Actor.Movement.HorizontalRotation);
				}
			}

			args.Player.State.SetState(PlayerStateId.Playing);

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
			if (!Room.CanStartMatch) {
				Room.HasRoundEnded = true;
			}
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs args) {
			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendPlayerKilled(args.AttackerCmid, args.VictimCmid, (byte)args.ItemClass, args.Damage, (byte)args.Part, args.Direction);

				if (peer.Actor.Cmid.CompareTo(args.VictimCmid) == 0) {
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

			args.Player.State.ResetState();
			args.Player.State.SetState(PlayerStateId.Playing);
		}
	}
}