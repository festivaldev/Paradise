using log4net;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public class DebugGameRoomState : GameRoomState {
		private static readonly ILog Log = LogManager.GetLogger(typeof(DebugGameRoomState));

		private Timer frameTimer;
		private ushort _frame;

		public DebugGameRoomState(BaseGameRoom room) : base(room) { }

		public override void OnEnter() {
			Room.PlayerJoined += OnPlayerJoined;

			_frame = 6;
			frameTimer = new Timer(Room.Loop, 1000 / 9.75f);
			frameTimer.Restart();
		}

		public override void OnExit() {
			Room.PlayerJoined -= OnPlayerJoined;
		}

		public override void OnResume() { }

		public override void OnUpdate() {
			bool updateMovements = frameTimer.Tick();
			if (updateMovements) {
				_frame++;
			}

			var deltas = new List<GameActorInfoDelta>(Room.Players.Count);
			var positions = new List<PlayerMovement>(Room.Players.Count);

			foreach (var peer in Room.Peers) {
				if (peer.Actor.Delta.Changes.Count > 0) {
					foreach (var kvp in peer.Actor.Delta.Changes) {
						Log.Info($"delta [{kvp.Key}({(int)kvp.Key})] => {kvp.Value}");
					}

					peer.Actor.Delta.UpdateDeltaMask();

					deltas.Add(peer.Actor.Delta);
				}

				if (peer.Actor.Info.IsAlive && updateMovements) {
					positions.Add(peer.Actor.Movement);
				}

				peer.State.Update();
			}

			foreach (var peer in Room.Peers) {
				if (positions.Count > 0) {
					peer.Events.Game.SendAllPlayerPositions(positions, _frame);
				}

				if (deltas.Count > 0) {
					peer.Events.Game.SendAllPlayerDeltas(deltas);
				}
			}

			foreach (var peer in Room.Peers) {
				peer.Actor.Info.Delta.Changes.Clear();
			}
		}


		private void OnPlayerJoined(object sender, PlayerJoinedEventArgs e) {
			var player = e.Player;

			foreach (var peer in Room.Peers) {
				peer.Events.Game.SendPlayerJoinedGame(player.Actor.Info, player.Actor.Movement);
				//otherPeer.KnownActors.Add(e.Player.Actor.Cmid);
			}

			//if (Room.Players.Count > 1) {
			//	Room.State.SetState(GameRoomState.Id.Countdown);
			//} else {
			player.State.SetState(GamePeerState.Id.WaitingForPlayers);
			//}

			//PrepareAndSpawnPlayer(e.Player);
		}

		private void PrepareAndSpawnPlayer(GamePeer player) {
			//var point = Room.SpawnManager.Get(player.Actor.Team);
			var movement = player.Actor.Movement;
			//movement.Position = point.Position;
			//movement.HorizontalRotation = point.Rotation;

			//Debug.Assert(player.Actor.Info.PlayerId == player.Actor.Movement.Number);

			/*
                This prepares the client for the next round and enables match start
                countdown thingy.
             */
			player.State.SetState(GamePeerState.Id.WaitingForPlayers);

			// Reset stats, so if the player is rejoining they do not retain their previous match stats.
			//player.WeaponStats = new Dictionary<int, WeaponStats>();
			//player.CurrentLifeStats = new StatsCollectionView();
			//player.StatsPerLife = new List<StatsCollectionView>();
			//player.TotalStats = new StatsCollectionView();
			//player.Lifetimes = new List<TimeSpan>();

			// Load their loadout.
			// Previously, it would use their cached loadout.
			//player.Loadout = player.Web.GetLoadout();

			var actorView = new GameActorInfo {
				// we don't want their team to change
				TeamID = player.Actor.Team,

				Health = 100,

				Deaths = 0,
				Kills = 0,

				Level = player.Member.UberstrikeMemberView.PlayerStatisticsView.Level,

				Channel = ChannelType.Steam,
				PlayerState = PlayerStates.None,
				SkinColor = Color.white,

				Ping = (ushort)(player.RoundTripTime / 2),

				Cmid = player.Member.CmuneMemberView.PublicProfile.Cmid,
				ClanTag = player.Member.CmuneMemberView.PublicProfile.GroupTag,
				AccessLevel = player.Member.CmuneMemberView.PublicProfile.AccessLevel,
				PlayerName = player.Member.CmuneMemberView.PublicProfile.Name,
			};

			/* Set the gears of the character. */
			/* Holo */
			actorView.Gear[0] = (int)player.Loadout.Webbing;
			actorView.Gear[1] = player.Loadout.Head;
			actorView.Gear[2] = player.Loadout.Face;
			actorView.Gear[3] = player.Loadout.Gloves;
			actorView.Gear[4] = player.Loadout.UpperBody;
			actorView.Gear[5] = player.Loadout.LowerBody;
			actorView.Gear[6] = player.Loadout.Boots;

			/* Sets the weapons of the character. */
			actorView.Weapons[0] = player.Loadout.MeleeWeapon;
			actorView.Weapons[1] = player.Loadout.Weapon1;
			actorView.Weapons[2] = player.Loadout.Weapon2;
			actorView.Weapons[3] = player.Loadout.Weapon3;

			var number = player.Actor.Number;
			var actor = new GameActor(actorView);
			player.Room = Room;
			player.Actor = actor;
			player.Actor.Number = number;
			//player.LifeStart = DateTime.Now.TimeOfDay;

			/* Let all peers know that the player has joined the game. */
			foreach (var otherPeer in Room.Peers) {
				//if (!otherPeer.KnownActors.Contains(player.Actor.Cmid)) {
					/*
                        PlayerJoinedGame event tells the client to initiate the character and register it
                        in its player list and update the team player number counts.
                     */
					otherPeer.Events.Game.SendPlayerJoinedGame(player.Actor.Info, movement);
					//otherPeer.KnownActors.Add(player.Actor.Cmid);
				//}

				otherPeer.Events.Game.SendPlayerRespawned(player.Actor.Cmid, movement.Position, movement.HorizontalRotation);

				if (otherPeer.Actor.Cmid != player.Actor.Cmid) {
					player.Events.Game.SendPlayerJoinedGame(otherPeer.Actor.Info, otherPeer.Actor.Movement);
					player.Events.Game.SendPlayerRespawned(otherPeer.Actor.Cmid, otherPeer.Actor.Movement.Position, otherPeer.Actor.Movement.HorizontalRotation);
				}
			}



			//Log.Debug($"Spawned: {player.Actor.Cmid} at: {point}");
		}
	}
}
