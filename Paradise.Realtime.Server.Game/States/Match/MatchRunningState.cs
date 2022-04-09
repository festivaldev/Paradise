using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.Core.Types;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				player.GameEvents.SendMatchStart(Room.RoundNumber, Room.RoundEndTime);
				player.State.SetState(PlayerStateId.Playing);
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
			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPlayerJoinedGame(args.Player.Actor.Info, args.Player.Actor.Movement);
			}

			PrepareAndSpawnPlayer(args.Player);
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
			var player = args.Player;

			player.Actor.Info.Health = 100;
			player.Actor.Info.ArmorPoints = player.Actor.Info.ArmorPointCapacity;
			player.Actor.Info.PlayerState = PlayerStates.Ready;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var peer in Room.Peers) {
				peer.GameEvents.SendPlayerRespawned(player.Actor.Cmid, spawn.Position, spawn.Rotation);
			}

			player.State.SetState(PlayerStateId.Playing);
		}

		private void OnMatchEnded(object sender, EventArgs args) {
			Room.State.SetState(GameStateId.EndOfMatch);
		}
		#endregion

		private void PrepareAndSpawnPlayer(GamePeer player) {
			player.State.SetState(PlayerStateId.PrepareForMatch);

			player.Actor.Info.Kills = 0;
			player.Actor.Info.Deaths = 0;

			var spawn = Room.SpawnPointManager.Get(player.Actor.Team);

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
