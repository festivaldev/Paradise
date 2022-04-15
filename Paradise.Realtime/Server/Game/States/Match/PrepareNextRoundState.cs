using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	internal class PrepareNextRoundState : BaseMatchState {
		private Dictionary<TeamID, List<SpawnPoint>> SpawnPointsInUse = new Dictionary<TeamID, List<SpawnPoint>>();

		private Countdown MatchStartCountdown;

		public PrepareNextRoundState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;
			Room.PlayerLeft += OnPlayerLeft;

			Room.HasRoundEnded = false;

			foreach (var player in Room.Players) {
				PrepareAndSpawnPlayer(player);
			}

			Task t = Task.Run(async () => {
				await Task.Delay(30);

				MatchStartCountdown = new Countdown(Room.Loop, 5, 0);
				MatchStartCountdown.Counted += OnCountdownCounted;
				MatchStartCountdown.Completed += OnCountdownCompleted;

				MatchStartCountdown.Restart();
			});
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
			Room.PlayerLeft -= OnPlayerLeft;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			MatchStartCountdown?.Tick();
		}

		#region Handlers
		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs args) {
			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerJoinedGame(args.Player.Actor.Info, args.Player.Actor.Movement);
			}

			PrepareAndSpawnPlayer(args.Player);
		}

		private void OnPlayerLeft(object sender, PlayerLeftEventArgs args) {
			if (!Room.CanStartMatch) {
				Room.State.SetState(GameStateId.EndOfMatch);
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
		#endregion

		private void PrepareAndSpawnPlayer(GamePeer player) {
			player.State.SetState(PlayerStateId.PrepareForMatch);

			player.Actor.Info.Kills = 0;
			player.Actor.Info.Deaths = 0;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			if (!SpawnPointsInUse.ContainsKey(player.Actor.Team)) {
				SpawnPointsInUse[player.Actor.Team] = new List<SpawnPoint> { spawn };
			} else {
				while (SpawnPointsInUse[player.Actor.Team].Contains(spawn)) {
					spawn = Room.SpawnPointManager.Get(player.Actor.Team);
				}

				SpawnPointsInUse[player.Actor.Team].Add(spawn);
			}

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			player.Loadout = new UserWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetLoadout(player.AuthToken);

			player.Actor.Info.Gear[0] = (int)player.Loadout.Webbing; // Holo
			player.Actor.Info.Gear[1] = player.Loadout.Head;
			player.Actor.Info.Gear[2] = player.Loadout.Face;
			player.Actor.Info.Gear[3] = player.Loadout.Gloves;
			player.Actor.Info.Gear[4] = player.Loadout.UpperBody;
			player.Actor.Info.Gear[5] = player.Loadout.LowerBody;
			player.Actor.Info.Gear[6] = player.Loadout.Boots;

			byte armorPointCapacity = 0;
			foreach (var armor in player.Actor.Info.Gear) {
				if (armor == 0) continue;

				var gear = default(UberStrikeItemGearView);
				if (Room.ShopManager.GearItems.TryGetValue(armor, out gear)) {
					armorPointCapacity = (byte)Math.Min(200, armorPointCapacity + gear.ArmorPoints);
				}
			}

			player.Actor.Info.ArmorPointCapacity = armorPointCapacity;
			player.Actor.Info.ArmorPoints = player.Actor.Info.ArmorPointCapacity;

			player.Actor.Info.Weapons[0] = player.Loadout.MeleeWeapon;
			player.Actor.Info.Weapons[1] = player.Loadout.Weapon1;
			player.Actor.Info.Weapons[2] = player.Loadout.Weapon2;
			player.Actor.Info.Weapons[3] = player.Loadout.Weapon3;

			if (player.Actor.Info.Weapons[1] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 1;
			} else if (player.Actor.Info.Weapons[2] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 2;
			} else if (player.Actor.Info.Weapons[3] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 3;
			} else if (player.Actor.Info.Weapons[0] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 0;
			}

			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerRespawned(player.Actor.Info.Cmid, player.Actor.Movement.Position, player.Actor.Movement.HorizontalRotation);
			}
		}
	}
}
