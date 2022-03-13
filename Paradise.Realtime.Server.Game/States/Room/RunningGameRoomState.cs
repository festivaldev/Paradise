using log4net;
using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	public class RunningGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(RunningGameRoomState));

		private ushort Frame = 0;
		private TimeSpan DepleteTime;

		public RunningGameRoomState(BaseGameRoom room) : base(room) {

		}

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerRespawned += OnPlayerRespawned;
			Room.PlayerKilled += OnPlayerKilled;

			Room.EndTime = Environment.TickCount + (Room.MetaData.TimeLimit * 1000);

			foreach (var player in Room.Players) {
				player.State.SetState(GamePeerState.Id.Playing);
			}

			Room.RoundNumber++;
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerRespawned -= OnPlayerRespawned;
			Room.PlayerKilled -= OnPlayerKilled;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			Room.PowerUpManager.Update();

			var deltas = new List<GameActorInfoDelta>(Room.Peers.Count);
			var positions = new List<PlayerMovement>(Room.Players.Count);

			foreach (var player in Room.Players) {
				positions.Add(player.Actor.Movement);

				if (player.Actor.Info.ArmorPoints > player.Actor.Info.ArmorPointCapacity && DateTime.Now.TimeOfDay >= DepleteTime) {
					player.Actor.Info.ArmorPoints--;
				}

				if (player.Actor.Info.Health > 100 && DateTime.Now.TimeOfDay >= DepleteTime) {
					player.Actor.Info.Health--;
				}

				if (player.Actor.Damage.Count > 0) {
					player.Events.Room.SendDamageEvent(player.Actor.Damage);
					player.Actor.Damage.Clear();
				}

				player.State.Update();
			}

			foreach (var peer in Room.Peers) {
				peer.Events.Room.SendAllPlayerDeltas(deltas);
				peer.Events.Room.SendAllPlayerPositions(positions, Frame);
			}

			if (DateTime.Now.TimeOfDay >= DepleteTime) {
				DepleteTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(1));
			}
		}

		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			///* Sync the power ups. */
			//e.Player.Events.Room.SendSetPowerupState(Room.PowerUpManager.Respawning);
			//e.Player.State.SetState(GamePeerState.Id.Playing);

			//Room.Spawn(e.Player);
		}

		private void OnPlayerRespawned(object sender, PlayerRespawnedEventArgs e) {
		
		}

		private void OnPlayerKilled(object sender, PlayerKilledEventArgs e) {
			
		}
	}
}
