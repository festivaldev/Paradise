using log4net;
using Paradise.Core.Models;

namespace Paradise.Realtime.Server.Game {
	public class WaitingForPlayersGameState : GameState {
		public WaitingForPlayersGameState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
			//Room.PlayerKilled += OnPlayerKilled;
			//Room.PlayerRespawned += OnPlayerRespawned;
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
			//Room.PlayerKilled -= OnPlayerKilled;
			//Room.PlayerRespawned -= OnPlayerRespawned;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }



		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			var player = e.Player;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerJoinedGame(player.Actor.Info, player.Actor.Movement);
			}

			if (Room.CanStartMatch) {
				Room.State.SetState(GameStateId.Countdown);
			} else {
				player.State.SetState(PlayerStateId.WaitingForPlayers);
			}
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs e) {
			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendPlayerKilled(e.AttackerCmid, e.VictimCmid, (byte)e.ItemClass, e.Damage, (byte)e.Part, e.Direction);

				if (peer.Actor.Cmid == e.VictimCmid) {
					peer.State.SetState(PlayerStateId.Killed);
					//peer.GameEvents.SendPlayerRespawnCountdown(0);
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

			player.State.SetState(PlayerStateId.WaitingForPlayers);
			//player.Events.Game.SendWaitingForPlayers();
		}
	}
}