using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.WebServices.Client;
using System;

namespace Paradise.Realtime.Server.Game {
	public class MatchRunningGameState : GameState {
		private Countdown MatchEndCountdown;

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

			MatchEndCountdown?.Tick();
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
			MatchEndCountdown = new Countdown(Room.Loop, 3, 0);
			MatchEndCountdown.Completed += OnMatchEndCountdownCompleted;
			MatchEndCountdown.Restart();

			foreach (var peer in Room.Players) {
				peer.GameEvents.SendTeamWins(Room.WinningTeam);
				peer.State.SetState(PlayerStateId.AfterRound);
			}
		}

		private void PrepareAndSpawnPlayer(GamePeer peer) {
			var spawn = Room.SpawnPointManager.Get(peer.Actor.Team);

			peer.Actor.Movement.Position = spawn.Position;
			peer.Actor.Movement.HorizontalRotation = spawn.Rotation;

			peer.State.SetState(PlayerStateId.Playing);

			peer.Loadout = new UserWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetLoadout(peer.AuthToken);

			peer.Actor.Info.Gear[0] = (int)peer.Loadout.Webbing; // Holo
			peer.Actor.Info.Gear[1] = peer.Loadout.Head;
			peer.Actor.Info.Gear[2] = peer.Loadout.Face;
			peer.Actor.Info.Gear[3] = peer.Loadout.Gloves;
			peer.Actor.Info.Gear[4] = peer.Loadout.UpperBody;
			peer.Actor.Info.Gear[5] = peer.Loadout.LowerBody;
			peer.Actor.Info.Gear[6] = peer.Loadout.Boots;

			byte armorPointCapacity = 0;
			foreach (var armor in peer.Actor.Info.Gear) {
				if (armor == 0) continue;

				var gear = default(UberStrikeItemGearView);
				if (Room.ShopManager.GearItems.TryGetValue(armor, out gear)) {
					armorPointCapacity = (byte)Math.Min(200, armorPointCapacity + gear.ArmorPoints);
				}
			}

			peer.Actor.Info.ArmorPointCapacity = armorPointCapacity;
			peer.Actor.Info.ArmorPoints = peer.Actor.Info.ArmorPointCapacity;

			peer.Actor.Info.Weapons[0] = peer.Loadout.MeleeWeapon;
			peer.Actor.Info.Weapons[1] = peer.Loadout.Weapon1;
			peer.Actor.Info.Weapons[2] = peer.Loadout.Weapon2;
			peer.Actor.Info.Weapons[3] = peer.Loadout.Weapon3;

			if (peer.Actor.Info.Weapons[1] > 0) {
				peer.Actor.Info.CurrentWeaponSlot = 1;
			} else if (peer.Actor.Info.Weapons[2] > 0) {
				peer.Actor.Info.CurrentWeaponSlot = 2;
			} else if (peer.Actor.Info.Weapons[3] > 0) {
				peer.Actor.Info.CurrentWeaponSlot = 3;
			} else if (peer.Actor.Info.Weapons[0] > 0) {
				peer.Actor.Info.CurrentWeaponSlot = 0;
			}

			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerRespawned(peer.Actor.Info.Cmid, peer.Actor.Movement.Position, peer.Actor.Movement.HorizontalRotation);
			}
		}

		private void OnMatchEndCountdownCompleted() {
			Room.State.SetState(GameStateId.EndOfMatch);
		}
	}
}