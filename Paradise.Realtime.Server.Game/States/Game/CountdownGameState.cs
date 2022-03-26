using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.WebServices.Client;
using System;

namespace Paradise.Realtime.Server.Game {
	public class CountdownGameState : GameState {
		private Countdown Countdown;

		public CountdownGameState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;

			Room.WinningTeam = TeamID.NONE;

			foreach (var player in Room.Players) {
				PrepareAndSpawnPlayer(player);
			}

			// We need to add an additional count because the game client seems to ignore the first count
			OnCountdownCounted(5);

			Countdown = new Countdown(Room.Loop, 5, 0);
			Countdown.Counted += OnCountdownCounted;
			Countdown.Completed += OnCountdownCompleted;

			Countdown.Restart();
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			Countdown?.Tick();
		}



		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			PrepareAndSpawnPlayer(e.Player);
		}

		private void PrepareAndSpawnPlayer(GamePeer peer) {
			var spawn = Room.SpawnPointManager.Get(peer.Actor.Team);

			peer.Actor.Movement.Position = spawn.Position;
			peer.Actor.Movement.HorizontalRotation = spawn.Rotation;

			peer.State.SetState(PlayerStateId.Countdown);

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

		private void OnCountdownCounted(int count) {
			foreach (var peer in Room.Players) {
				peer.GameEvents.SendMatchStartCountdown((byte)count);
			}
		}

		private void OnCountdownCompleted() {
			Room.State.SetState(GameStateId.MatchRunning);
		}
	}
}