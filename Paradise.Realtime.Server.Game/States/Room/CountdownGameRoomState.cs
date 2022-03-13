using log4net;
using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Realtime.Server.Game {
	public class CountdownGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(CountdownGameRoomState));

		private Countdown Countdown;

		public CountdownGameRoomState(BaseGameRoom room) : base(room) {
			Countdown = new Countdown(Room.Loop, 5, 0);
			Countdown.Counted += OnCountdownCounted;
			Countdown.Completed += OnCountdownCompleted;
		}

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;

			foreach (var player in Room.Players) {
				player.State.SetState(GamePeerState.Id.Countdown);
			}

			Countdown.Restart();
		}

		public override void OnExit() { }

		public override void OnResume() { }

		public override void OnUpdate() {
			Countdown.Tick();
		}


		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			var player = e.Player;

			var point = Room.SpawnPointManager.Get(player.Actor.Team);
			var movement = player.Actor.Movement;
			movement.Position = point.Position;
			movement.HorizontalRotation = point.Rotation;

			Debug.Assert(player.Actor.Info.PlayerId == player.Actor.Movement.Number);


			player.State.SetState(GamePeerState.Id.Countdown);

			// Reset stats, so if the player is rejoining they do not retain their previous match stats.
			//player.WeaponStats = new Dictionary<int, WeaponStats>();
			//player.CurrentLifeStats = new StatsCollectionView();
			//player.StatsPerLife = new List<StatsCollectionView>();
			//player.TotalStats = new StatsCollectionView();
			//player.Lifetimes = new List<TimeSpan>();

			player.Loadout = new UserWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetLoadout(player.AuthToken);

			var actorView = new GameActorInfo {
				TeamID = player.Actor.Team,

				Health = 100,

				Deaths = 0,
				Kills = 0,

				Level = player.Member.UberstrikeMemberView.PlayerStatisticsView.Level,

				Channel = ChannelType.Steam,
				PlayerState = PlayerStates.None,

				Ping = (ushort)(player.RoundTripTime / 2),

				Cmid = player.Member.CmuneMemberView.PublicProfile.Cmid,
				ClanTag = player.Member.CmuneMemberView.PublicProfile.GroupTag,
				AccessLevel = player.Member.CmuneMemberView.PublicProfile.AccessLevel,
				PlayerName = player.Member.CmuneMemberView.PublicProfile.Name,
			};

			actorView.Gear[0] = (int)player.Loadout.Webbing; // Holo
			actorView.Gear[1] = player.Loadout.Head;
			actorView.Gear[2] = player.Loadout.Face;
			actorView.Gear[3] = player.Loadout.Gloves;
			actorView.Gear[4] = player.Loadout.UpperBody;
			actorView.Gear[5] = player.Loadout.LowerBody;
			actorView.Gear[6] = player.Loadout.Boots;


			byte armorPointCapacity = 0;
			foreach (var armor in actorView.Gear) {
				if (armor == 0) {
					continue;
				}

				var gear = default(UberStrikeItemGearView);
				if (Room.ShopManager.GearItems.TryGetValue(armor, out gear)) {
					armorPointCapacity = (byte)Math.Min(200, armorPointCapacity + gear.ArmorPoints);
				} else {
					Log.Debug($"Could not find gear with ID {armor}.");
				}
			}

			actorView.ArmorPointCapacity = armorPointCapacity;
			actorView.ArmorPoints = actorView.ArmorPointCapacity;

			actorView.Weapons[0] = player.Loadout.MeleeWeapon;
			actorView.Weapons[1] = player.Loadout.Weapon1;
			actorView.Weapons[2] = player.Loadout.Weapon2;
			actorView.Weapons[3] = player.Loadout.Weapon3;

			var number = player.Actor.Number;
			var actor = new GameActor(actorView);
			player.Room = Room;
			player.Actor = actor;
			player.Actor.Number = number;
			player.LifeStart = DateTime.Now.TimeOfDay;

			foreach (var otherPeer in Room.Peers) {
				if (!otherPeer.KnownActors.Contains(player.Actor.Cmid)) {
					otherPeer.Events.Room.SendPlayerJoinedGame(player.Actor.Info, movement);
					otherPeer.KnownActors.Add(player.Actor.Cmid);
				}

				otherPeer.Events.Room.SendPlayerRespawned(player.Actor.Cmid, movement.Position, movement.HorizontalRotation);
			}

			Log.Debug($"Spawned: {player.Actor.Cmid} at: {point}");
		}

		private void OnCountdownCounted(int count) {
			foreach (var player in Room.Players) {
				player.Events.Room.SendMatchStartCountdown((byte)count);
			}
		}

		private void OnCountdownCompleted() {
			Room.State.SetState(GameRoomState.Id.Running);
		}
	}
}
