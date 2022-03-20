using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class DebugGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(DebugGameRoomState));

		public DebugGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Log.Info("entered debug state");
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerKilled += OnPlayerKilled;
			Room.PlayerRespawned += OnPlayerRespawned;
		}

		public override void OnExit() {
			Log.Info("left debug state");

			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerKilled -= OnPlayerKilled;
			Room.PlayerRespawned -= OnPlayerRespawned;
		}

		public override void OnResume() { }

		public override void OnUpdate() { }



		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			Log.Info("player joined");

			var player = e.Player;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var otherPeer in Room.Peers) {
				otherPeer.Events.Game.SendPlayerJoinedGame(player.Actor.Info, player.Actor.Movement);
			}

			player.State.SetState(GamePeerState.Id.WaitingForPlayers);
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs e) {
			foreach (var peer in Room.Peers) {
				peer.Events.Game.SendPlayerKilled(e.AttackerCmid, e.VictimCmid, (byte)e.ItemClass, e.Damage, (byte)e.Part, e.Direction);

				if (peer.Actor.Cmid == e.VictimCmid) {
					peer.Events.Game.SendPlayerRespawnCountdown(0);
				}
			}

		}

		private void OnPlayerRespawned(object sender, PlayerRespawnedEventArgs e) {
			Log.Info("player respawned");

			var player = e.Player;

			player.Actor.Info.Health = 100;
			player.Actor.Info.ArmorPoints = player.Actor.Info.ArmorPointCapacity;
			player.Actor.Info.PlayerState = PlayerStates.None;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var otherPeer in Room.Peers) {
				otherPeer.Events.Game.SendPlayerRespawned(player.Actor.Cmid, spawn.Position, spawn.Rotation);
			}

			/* Switch to previous state which should be 'playing state'. */
			player.State.SetState(GamePeerState.Id.WaitingForPlayers);
		}
	}
}
