using log4net;
using Paradise.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Paradise.Realtime.Server.Game {
	public class DebugGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(DebugGameRoomState));

		private Timer frameTimer;
		private ushort _frame;

		public DebugGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Log.Info("entered debug state");
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerKilled += OnPlayerKilled;
			Room.PlayerRespawned += OnPlayerRespawned;

			_frame = 6;
			frameTimer = new Timer(Room.Loop, 1000 / 9.25f);
			frameTimer.Restart();
		}

		public override void OnExit() {
			Log.Info("left debug state");

			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerKilled -= OnPlayerKilled;
			Room.PlayerRespawned -= OnPlayerRespawned;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			var updatePositions = frameTimer.Tick();
			if (updatePositions) _frame++;

			var positions = new List<PlayerMovement>();
			var deltas = new List<GameActorInfoDelta>();

			foreach (var peer in Room.Peers) {
				var actor = peer.Actor;
				peer.State.Update();

				if (Room.Players.Contains(actor)) {
					var delta = actor.Delta;

					if (delta.Changes.Count > 0) {
						delta.UpdateDeltaMask();
						deltas.Add(delta);
					}

					if (actor.Damage.Count > 0) {
						peer.Events.Game.SendDamageEvent(actor.Damage);
						actor.Damage.Clear();
					}

					if (updatePositions && actor.Info.IsAlive) {
						positions.Add(actor.Movement);
					}
				}
			}

			if (deltas.Count > 0) {
				foreach (var peer in Room.Peers) {
					peer.Events.Game.SendAllPlayerDeltas(deltas);
				}

				foreach (var delta in deltas) {
					delta.Reset();
				}

				deltas.Clear();
			}

			if (positions.Count > 0 && updatePositions) {
				foreach (var peer in Room.Peers) {
					peer.Events.Game.SendAllPlayerPositions(positions, _frame);
				}

				positions.Clear();
			}
		}



		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			Log.Info("player joined");

			var player = e.Player;

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
			/* Let all peers know that the player has respawned. */
			e.Player.Actor.Info.Health = 100;
			e.Player.Actor.Info.PlayerState &= ~PlayerStates.Dead;

			//var spawn = Room.SpawnManager.Get(e.Player.Actor.Team);
			foreach (var otherPeer in Room.Peers) {
				otherPeer.Events.Game.SendPlayerRespawned(e.Player.Actor.Cmid, e.Player.Actor.Movement.Position, e.Player.Actor.Movement.HorizontalRotation);
			}

			/* Switch to previous state which should be 'playing state'. */
			e.Player.State.SetState(GamePeerState.Id.WaitingForPlayers);
		}
	}
}
