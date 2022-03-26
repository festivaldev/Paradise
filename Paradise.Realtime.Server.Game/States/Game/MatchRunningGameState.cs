using Paradise.Core.Models;
using System;

namespace Paradise.Realtime.Server.Game {
	public class MatchRunningGameState : GameState {
		public MatchRunningGameState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerKilled += OnPlayerKilled;
			Room.PlayerRespawned += OnPlayerRespawned;
			Room.MatchEnded += OnMatchEnded;

			Room.RoundStartTime = Environment.TickCount;
			Room.RoundEndTime = Room.RoundStartTime + (Room.MetaData.TimeLimit * 1000);

			foreach (var peer in Room.Players) {
				peer.State.SetState(PlayerStateId.Playing);
			}
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
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



		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerJoinedGame(e.Player.Actor.Info, e.Player.Actor.Movement);
			}

			PrepareAndSpawnPlayer(e.Player);
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs e) {
			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendPlayerKilled(e.AttackerCmid, e.VictimCmid, (byte)e.ItemClass, e.Damage, (byte)e.Part, e.Direction);

				if (peer.Actor.Cmid == e.VictimCmid) {
					peer.State.SetState(PlayerStateId.Killed);
				}
			}
		}

		private void OnPlayerRespawned(object sender, PlayerRespawnedEventArgs e) {
			var player = e.Player;

			player.Actor.Info.Health = 100;
			player.Actor.Info.ArmorPoints = player.Actor.Info.ArmorPointCapacity;
			player.Actor.Info.PlayerState = PlayerStates.Ready;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerRespawned(player.Actor.Cmid, spawn.Position, spawn.Rotation);
			}

			player.State.SetState(PlayerStateId.Playing);
		}

		private void OnMatchEnded(object sender, EventArgs e) {
			Room.State.SetState(GameStateId.EndOfMatch);
		}
	}
}