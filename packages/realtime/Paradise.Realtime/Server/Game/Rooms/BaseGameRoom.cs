/*
 * Copyright (C) 2017, 2022-2026 Team FESTIVAL
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Cmune.DataCenter.Common.Entities;
using log4net;
using Paradise.Realtime.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UberStrike.Core.Models;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;
using static Paradise.WebSocket;

namespace Paradise.Realtime.Server.Game {
	public abstract partial class BaseGameRoom : IRoom<GamePeer>, IDisposable {
		public const float TICK_TRATE = 9.5f;
		public const float ARMOR_ABSORPTION = 0.66f;

		protected static readonly ILog Log = LogManager.GetLogger(nameof(BaseGameRoom));
		protected static readonly ILog ChatLog = LogManager.GetLogger("ChatLog");

		private readonly BaseGameRoom.OperationHandler OpHandler;

		protected object Lock { get; } = new object();
		private static readonly ProfanityFilter.ProfanityFilter ProfanityFilter = new ProfanityFilter.ProfanityFilter();

		private bool IsDisposed = false;

		public GameRoomData MetaData { get; private set; }
		public int RoomId {
			get { return MetaData.Number; }
			set { MetaData.Number = value; }
		}

		private string password;
		public string Password {
			get { return password; }
			set {
				MetaData.IsPasswordProtected = !string.IsNullOrEmpty(value);

				password = value;
			}
		}

		public bool IsTeamGame {
			get {
				return MetaData.GameMode == GameModeType.TeamDeathMatch || MetaData.GameMode == GameModeType.EliminationMode;
			}
		}

		private readonly List<GamePeer> peers = new List<GamePeer>();
		public IReadOnlyList<GamePeer> Peers {
			get {
				lock (peers) {
					return peers.ToArray();
				}
			}
		}

		private readonly List<GamePeer> players = new List<GamePeer>();
		public IReadOnlyList<GamePeer> Players {
			get {
				lock (players) {
					return players.ToArray();
				}
			}
		}

		public int RoundNumber;
		public int RoundStartTime;
		public int RoundEndTime;
		public List<TimeSpan> RoundDurations = new List<TimeSpan>();
		public bool HasRoundEnded;

		private byte NextPlayerId;

		public Loop Loop { get; private set; }
		public ILoopScheduler Scheduler { get; private set; }

		private readonly Timer frameTimer;
		private ushort gameFrame;

		public StateMachine<GameStateId> State { get; private set; } = new StateMachine<GameStateId>();

		public SpawnPointManager SpawnPointManager { get; private set; }
		public ShopManager ShopManager { get; private set; }
		public PowerUpManager PowerUpManager { get; private set; }
		public StatisticsManager StatisticsManager { get; private set; }
		public AchievementManager AchievementManager { get; private set; }

		public int WinningCmid = 0;
		public TeamID WinningTeam = TeamID.NONE;

		public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
		public event EventHandler<PlayerLeftEventArgs> PlayerLeft;
		public event EventHandler<PlayerKilledEventArgs> PlayerKilled;
		public event EventHandler<PlayerRespawnedEventArgs> PlayerRespawned;
		public event EventHandler<PlayerJoinedEventArgs> SpectatorJoined;
		public event EventHandler<EventArgs> MatchStarted;
		public event EventHandler<EventArgs> MatchEnded;

		public BaseGameRoom(GameRoomData metaData, ILoopScheduler scheduler) {
			MetaData = metaData ?? throw new ArgumentNullException(nameof(metaData));
			Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

			OpHandler = new OperationHandler(this);

			SpawnPointManager = new SpawnPointManager(this);
			ShopManager = new ShopManager();
			PowerUpManager = new PowerUpManager(this);
			StatisticsManager = new StatisticsManager(this);
			AchievementManager = new AchievementManager(this);

			ShopManager.Load();

			Loop = new Loop(OnTick, OnTickError);

			/*
			 * * Expected interval between ticks by the client is 100ms
			 * (10 tick/s).
			 *
			 * * Lag extrapolation starts when the packets arrive at around
			 * 150ms late assuming the client receives a constant stream of
			 * packets of 100ms intervals. The threshold of 150ms is not
			 * constant and varies according to the measurements done by the
			 * client.
			 *
			 * * We're actually updating at 9.5 tick/s, because it seems the
			 * client can not truely handle packets at 10 tick/s. This is
			 * likely because sometimes packets gets "clogged" in the client's
			 * packet queue and since packets don't have a timestamp of when
			 * they were sent; it appears to be have been sent faster than
			 * 10 tick/s. This therefore causes the client to update the
			 * position of the players with little to no interpolation in an
			 * attempt to catch up.
			 *
			 * So we use a tick rate lower than that expected by the client to
			 * give it some breathing room and reduce the chance of packets
			 * building up in its packet queue to the point where it does hard
			 * position updates.
			 *
			 * * TLDR;
			 * A lower tick rate gives a smoother motion but more inaccurate
			 * positions.
			 * A higher tick rate gives a choppier motion but more accurate
			 * positions.
			 *
			 * - FICTURE7
			 */
			frameTimer = new Timer(Loop, 1000 / TICK_TRATE);

			State.RegisterState(GameStateId.None, null);
			State.RegisterState(GameStateId.WaitingForPlayers, new WaitingForPlayersState(this));
			State.RegisterState(GameStateId.PrepareNextRound, new PrepareNextRoundState(this));
			State.RegisterState(GameStateId.MatchRunning, new MatchRunningState(this));
			State.RegisterState(GameStateId.EndOfMatch, new EndOfMatchState(this));
			State.RegisterState(GameStateId.AfterRound, new AfterRoundState(this));

			Reset();

			Scheduler.Schedule(Loop);
		}

		public virtual void Reset() {
			gameFrame = 6;
			frameTimer.Restart();

			RoundNumber = 0;
			RoundStartTime = int.MinValue;
			RoundEndTime = int.MinValue;
			RoundDurations.Clear();

			UpdateNextPlayerID();

			foreach (var peer in Peers) {
				foreach (var otherPeer in Peers) {
					otherPeer.GameEventSender.SendPlayerLeftGame(peer.Actor.Cmid);
				}
			}

			players.Clear();

			State.ResetState();
			State.SetState(GameStateId.WaitingForPlayers);
		}

		public void Join(GamePeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			peer.LastOperationTime.Clear();
			peer.OperationSpamCounter.Clear();

			var actorInfo = new GameActorInfo {
				TeamID = TeamID.NONE,
				Health = 100,
				Level = XpPointsUtil.GetLevelForXp(peer.Member.UberstrikeMemberView.PlayerStatisticsView.Xp),
				Channel = ChannelType.Steam,
				PlayerState = PlayerStates.None,
				Ping = (ushort)(peer.RoundTripTime / 2),
				PlayerId = NextPlayerId,

				Cmid = peer.Member.CmuneMemberView.PublicProfile.Cmid,
				ClanTag = peer.Member.CmuneMemberView.PublicProfile.GroupTag,
				AccessLevel = peer.Member.CmuneMemberView.PublicProfile.AccessLevel,
				PlayerName = peer.Member.CmuneMemberView.PublicProfile.Name,
			};

			peer.Actor = new GameActor(peer, actorInfo);
			peer.Room = this;

			lock (peers) {
				if (peers.Find(_ => _.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0) != null) {
					peers.RemoveAll(_ => _.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0);
				}

				peers.Add(peer);
				UpdateNextPlayerID();
			}

			peer.Actor.Delta.Changes.Clear();

			peer.AddOperationHandler(OpHandler);

			peer.PeerEventSender.SendRoomEntered(MetaData);
			peer.State.SetState(PlayerStateId.Overview);

			MetaData.ConnectedPlayers = Peers.Count;

			Log.Info($"{peer.Actor.Name}({peer.Actor.Cmid}) joined {this}({this.MetaData.Number})");
			GameServerApplication.Instance.SocketClient?.SendSync(PacketType.PlayerJoinedRoom, (peer.Actor.ActorInfo, this.MetaData));
		}

		public void Leave(GamePeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			System.Diagnostics.Debug.Assert(peer.Room != null, "GamePeer is leaving room, but is not in any room.");
			System.Diagnostics.Debug.Assert(peer.Room == this, "GamePeer is leaving room, but is not leaving the correct room.");

			foreach (var otherPeer in Peers) {
				otherPeer.GameEventSender.SendPlayerLeftGame(peer.Actor.Cmid);
			}

			lock (peers) {
				peers.Remove(peer);
				players.Remove(peer);

				MetaData.ConnectedPlayers = Peers.Count;
			}

			Log.Info($"{peer.Actor.Name}({peer.Actor.Cmid}) left {this}({this.MetaData.Number})");

			if (peers.Count > 0) {
				GameServerApplication.Instance.SocketClient?.SendSync(PacketType.PlayerLeftRoom, (peer.Actor.ActorInfo, this.MetaData));
			}

			peer.State.SetState(PlayerStateId.None);
			peer.RemoveOperationHandler(OpHandler.Id);
			peer.Actor = null;
			peer.Room = null;

			OnPlayerLeft(new PlayerLeftEventArgs {
				Player = peer
			});
		}

		public abstract bool CanJoinMatch { get; }
		public abstract bool CanStartMatch { get; }
		public abstract void GetCurrentScore(out short killsRemaining, out short blueTeamScore, out short redTeamScore);

		private void OnTick() {
			var updatePositions = frameTimer.Tick();
			if (updatePositions)
				gameFrame++;

			State.Update();

			var positions = new List<PlayerMovement>();
			var deltas = new List<GameActorInfoDelta>();

			GamePeer[] peerList;
			lock (peers) {
				peerList = peers.ToArray();
			}

			HashSet<GamePeer> playerSet;
			lock (players) {
				playerSet = new HashSet<GamePeer>(players);
			}

			foreach (var peer in peerList) {
				if (peer.HasError) {
					peer.Disconnect();

					continue;
				}

				peer.Tick();
				peer.State.Update();

				var actor = peer.Actor;

				if (actor == null) {
					continue;
				}

				if (playerSet.Contains(peer)) {
					var delta = actor.Delta;

					if (delta.Changes.Count > 0) {
						deltas.Add(delta);
					}

					if (actor.Damage.Count > 0) {
						peer.GameEventSender.SendDamageEvent(actor.Damage);
						actor.Damage.Clear();
					}

					if (updatePositions && actor.ActorInfo.IsAlive && actor.UpdatePosition) {
						actor.UpdatePosition = false;
						positions.Add(actor.Movement);
					}
				}
			}

			if (deltas.Count > 0) {
				foreach (var peer in peerList) {
					peer.GameEventSender.SendAllPlayerDeltas(deltas);
				}

				foreach (var delta in deltas) {
					delta.Reset();
				}

				deltas.Clear();
			}

			if (positions.Count > 0 && updatePositions) {
				foreach (var peer in peerList) {
					peer.GameEventSender.SendAllPlayerPositions(positions, gameFrame);
				}

				positions.Clear();
			}

			if (HasRoundEnded && (State.CurrentStateId == GameStateId.PrepareNextRound || State.CurrentStateId == GameStateId.MatchRunning)) {
				OnMatchEnded(new EventArgs());
			}
		}

		private void OnTickError(Exception e) {
			Log.Error("loop tick error", e);
		}

		public void Dispose() {
			Dispose(true);
		}

		private void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				Scheduler.Unschedule(Loop);

				foreach (var peer in Peers) {
					foreach (var player in Players) {
						peer.GameEventSender.SendPlayerLeftGame(player.Actor.Cmid);
					}
				}

				foreach (var peer in Peers) {
					peer.Actor = null;
					peer.RemoveOperationHandler(OpHandler.Id);

					peer.Disconnect();
					peer.Dispose();
				}

				peers.Clear();
				players.Clear();
			}

			IsDisposed = true;
		}

		#region Player Management
		public void PreparePlayer(GamePeer player, bool isSpectator = false) {
			if (isSpectator) {
				player.Actor.ActorInfo.PlayerState = PlayerStates.Spectator;
			} else {
				player.Actor.ActorInfo.PlayerState = PlayerStates.None;
			}

			StatisticsManager.Add(player);

			player.Actor.ActorInfo.Kills = 0;
			player.Actor.ActorInfo.Deaths = 0;

			player.Loadout = UserWebServiceClient.Instance.GetLoadout(player.AuthToken);

			player.Actor.ActorInfo.Gear[0] = (int)player.Loadout.Webbing; // Holo
			player.Actor.ActorInfo.Gear[1] = player.Loadout.Head;
			player.Actor.ActorInfo.Gear[2] = player.Loadout.Face;
			player.Actor.ActorInfo.Gear[3] = player.Loadout.Gloves;
			player.Actor.ActorInfo.Gear[4] = player.Loadout.UpperBody;
			player.Actor.ActorInfo.Gear[5] = player.Loadout.LowerBody;
			player.Actor.ActorInfo.Gear[6] = player.Loadout.Boots;

			player.Actor.ActorInfo.Health = 100;

			byte armorPointCapacity = 0;
			foreach (var armor in player.Actor.ActorInfo.Gear) {
				if (armor == 0)
					continue;

				if (ShopManager.GearItems.TryGetValue(armor, out var gear)) {
					armorPointCapacity = (byte)Math.Min(200, armorPointCapacity + gear.ArmorPoints);
				}
			}

			player.Actor.ActorInfo.ArmorPointCapacity = armorPointCapacity;

			if (!GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.NoArmor, MetaData.GameFlags)) {
				player.Actor.ActorInfo.ArmorPoints = player.Actor.ActorInfo.ArmorPointCapacity;
			} else {
				player.Actor.ActorInfo.ArmorPoints = 0;
			}

			player.Actor.ActorInfo.Weapons[0] = player.Loadout.MeleeWeapon;

			if (!GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.MeleeOnly, MetaData.GameFlags)) {
				player.Actor.ActorInfo.Weapons[1] = player.Loadout.Weapon1;
				player.Actor.ActorInfo.Weapons[2] = player.Loadout.Weapon2;
				player.Actor.ActorInfo.Weapons[3] = player.Loadout.Weapon3;
			}

			if (player.Actor.ActorInfo.Weapons[1] > 0) {
				player.Actor.ActorInfo.CurrentWeaponSlot = 1;
			} else if (player.Actor.ActorInfo.Weapons[2] > 0) {
				player.Actor.ActorInfo.CurrentWeaponSlot = 2;
			} else if (player.Actor.ActorInfo.Weapons[3] > 0) {
				player.Actor.ActorInfo.CurrentWeaponSlot = 3;
			} else if (player.Actor.ActorInfo.Weapons[0] > 0) {
				player.Actor.ActorInfo.CurrentWeaponSlot = 0;
			}
		}

		public void SpawnPlayer(GamePeer player, bool joinGame) {
			Log.Debug($"is spectator: {player.Actor.ActorInfo.IsSpectator}");
			if (player.Actor.ActorInfo.IsSpectator)
				return;

			if (SpawnPointManager.TryGet(player.Actor.Team, out var spawn)) {
				if (!SpawnPointManager.SpawnPointsInUse.ContainsKey(player.Actor.Team)) {
					SpawnPointManager.SpawnPointsInUse[player.Actor.Team] = new List<SpawnPoint> { spawn };
				} else {
					if (State.CurrentStateId == GameStateId.WaitingForPlayers || State.CurrentStateId == GameStateId.PrepareNextRound) {
						while (SpawnPointManager.IsSpawnPointInUse(player.Actor.Team, spawn)) {
							SpawnPointManager.TryGet(player.Actor.Team, out spawn);
						}

						SpawnPointManager.SpawnPointsInUse[player.Actor.Team].Add(spawn);
					} else {
						if (player.Actor.PreviousSpawnPoints.Count >= SpawnPointManager.GetSpawnPointCount(player.Actor.Team)) {
							player.Actor.PreviousSpawnPoints.Clear();
						} else {
							while (player.HasSpawnedOnSpawnPoint(spawn)) {
								SpawnPointManager.TryGet(player.Actor.Team, out spawn);
							}
						}
					}
				}

				player.Actor.ActorInfo.PlayerState = PlayerStates.None;

				player.Actor.ActorInfo.Health = 100;

				byte armorPointCapacity = 0;
				foreach (var armor in player.Actor.ActorInfo.Gear) {
					if (armor == 0)
						continue;

					if (ShopManager.GearItems.TryGetValue(armor, out var gear)) {
						armorPointCapacity = (byte)Math.Min(200, armorPointCapacity + gear.ArmorPoints);
					}
				}

				player.Actor.ActorInfo.ArmorPointCapacity = armorPointCapacity;

				if (!GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.NoArmor, MetaData.GameFlags)) {
					player.Actor.ActorInfo.ArmorPoints = player.Actor.ActorInfo.ArmorPointCapacity;
				} else {
					player.Actor.ActorInfo.ArmorPoints = 0;
				}

				player.Actor.CurrentSpawnPoint = spawn;
				player.Actor.PreviousSpawnPoints.Add(spawn);

				player.Actor.Movement.Position = spawn.Position;
				player.Actor.Movement.HorizontalRotation = Conversion.Angle2Byte(spawn.Rotation.y);

				player.Actor.LastRespawnTime = DateTime.UtcNow;
				player.Actor.NextRespawnTime = DateTime.MinValue;

				foreach (var peer in Peers) {
					if (joinGame) {
						peer.GameEventSender.SendPlayerJoinedGame(player.Actor.ActorInfo, player.Actor.Movement);
						peer.GameEventSender.SendPlayerRespawned(player.Actor.Cmid, player.Actor.Movement.Position, player.Actor.Movement.HorizontalRotation);
					} else {
						peer.GameEventSender.SendPlayerRespawned(player.Actor.Cmid, player.Actor.Movement.Position, player.Actor.Movement.HorizontalRotation);
					}
				}
			}
		}

		protected GamePeer FindPeerWithCmid(int cmid) {
			lock (Lock) {
				foreach (var peer in Peers) {
					if (peer.Actor.Cmid == cmid) {
						return peer;
					}
				}
			}

			return null;
		}
		#endregion

		#region Events
		protected virtual void OnMatchStarted(EventArgs args) {
			MatchStarted?.Invoke(this, args);
		}

		protected virtual void OnMatchEnded(EventArgs args) {
			MatchEnded?.Invoke(this, args);
		}

		protected virtual void OnPlayerKilled(PlayerKilledEventArgs args) {
			PlayerKilled?.Invoke(this, args);
		}

		protected virtual void OnPlayerRespawned(PlayerRespawnedEventArgs args) {
			PlayerRespawned?.Invoke(this, args);
		}

		protected virtual void OnPlayerJoined(PlayerJoinedEventArgs args) {
			PlayerJoined?.Invoke(this, args);
		}

		protected virtual void OnPlayerLeft(PlayerLeftEventArgs args) {
			PlayerLeft?.Invoke(this, args);
		}
		#endregion

		#region Implementation of IGameRoomOperationsType
		private void JoinGame(GamePeer peer, TeamID team) {
			peer.Actor.Team = team;
			peer.Actor.ActorInfo.Health = 100;

			if (!GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.NoArmor, MetaData.GameFlags)) {
				peer.Actor.ActorInfo.ArmorPoints = peer.Actor.ActorInfo.ArmorPointCapacity;
			} else {
				peer.Actor.ActorInfo.ArmorPoints = 0;
			}

			peer.Actor.ActorInfo.Ping = (ushort)(peer.RoundTripTime / 2);
			peer.Actor.ActorInfo.PlayerState = PlayerStates.None;

			peer.Actor.ActorInfo.Kills = 0;
			peer.Actor.ActorInfo.Deaths = 0;

			if (!CanJoinMatch) {
				peer.Actor.ActorInfo.PlayerState = PlayerStates.Spectator;
			}

			lock (players) {
				if (players.FirstOrDefault(_ => _.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0) == null) {
					players.Add(peer);
				}
			}

			OnPlayerJoined(new PlayerJoinedEventArgs {
				Player = peer,
				Team = peer.Actor.Team
			});
		}

		private void JoinAsSpectator(GamePeer peer) {
			// Only accessible ingame if peer.Member.CmuneMemberView.PublicProfile.AccessLevel >= MemberAccessLevel.QA

			peer.State.SetState(PlayerStateId.Spectating);

			OnPlayerJoined(new PlayerJoinedEventArgs {
				Player = peer,
				Team = peer.Actor.Team
			});
		}

		private void PowerUpRespawnTimes(GamePeer peer, List<ushort> respawnTimes, List<Vector3> positions) {
			if (!PowerUpManager.IsLoaded) {
				PowerUpManager.Load(positions, respawnTimes);
			}
		}

		private void PowerUpPicked(GamePeer peer, int powerupId, byte type, byte value) {
			PowerUpManager.PickUp(peer, powerupId, (PickupItemType)type, value);
		}

		private void IncreaseHealthAndArmor(GamePeer peer, byte health, byte armor) {
			// Not supported
		}

		private void OpenDoor(GamePeer peer, int doorId) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0)
					continue;

				otherPeer.GameEventSender.SendDoorOpen(doorId);
			}
		}

		private void SpawnPositions(GamePeer peer, TeamID team, List<Vector3> positions, List<byte> rotations) {
			if (!SpawnPointManager.IsLoaded(team)) {
				SpawnPointManager.Load(team, positions, rotations);
			}
		}

		private void RespawnRequest(GamePeer peer) {
			if (peer.Actor.NextRespawnTime > DateTime.UtcNow)
				return;

			OnPlayerRespawned(new PlayerRespawnedEventArgs { Player = peer });
		}

		private void DirectHitDamage(GamePeer peer, int target, byte bodyPart, byte bullets) {
			if (State.CurrentStateId != GameStateId.MatchRunning)
				return;
			if ((peer.Actor.ActorInfo.PlayerState & PlayerStates.Dead) != 0)
				return;
			if (bullets <= 0)
				return;

			foreach (var player in Players) {
				if (player.Actor.Cmid != target)
					continue;
				if ((player.Actor.ActorInfo.PlayerState & PlayerStates.Dead) != 0)
					return;
				if (Math.Abs((player.Actor.LastRespawnTime - DateTime.UtcNow).TotalSeconds) < 2)
					return;

				if (!GameServerApplication.Instance.Configuration.GameplaySettings.AllowFriendlyFire) {
					if (peer.Actor.ActorInfo.TeamID == player.Actor.ActorInfo.TeamID && peer.Actor.ActorInfo.TeamID != TeamID.NONE)
						return;
				}

				var weapon = peer.Actor.CurrentWeapon;
				if (weapon != null) {
					var damage = (weapon.DamagePerProjectile * bullets);

					var part = (BodyPart)bodyPart;
					var bonus = weapon.CriticalStrikeBonus;
					if (bonus > 0) {
						if (part == BodyPart.Head || part == BodyPart.Nuts) {
							damage = (int)Math.Round(damage + (damage * (bonus / 100f)));
						}
					}

					var shortDamage = (short)damage;

					var victimPos = player.Actor.Movement.Position;
					var attackerPos = peer.Actor.Movement.Position;

					var direction = attackerPos - victimPos;
					var back = new Vector3(0, 0, -1);

					var angle = Vector3.Angle(direction, back);
					if (direction.x < 0)
						angle = 360 - angle;

					var byteAngle = Conversion.Angle2Byte(angle);

					if (player.Actor.ActorInfo.ArmorPoints > 0) {
						int originalArmor = player.Actor.ActorInfo.ArmorPoints;
						player.Actor.ActorInfo.ArmorPoints = (byte)Math.Max(0, player.Actor.ActorInfo.ArmorPoints - shortDamage);

						double diff = (originalArmor - player.Actor.ActorInfo.ArmorPoints) * ARMOR_ABSORPTION;
						shortDamage -= (short)diff;
					}

					player.Actor.Damage.AddDamage(byteAngle, shortDamage, bodyPart, 0, 0);
					player.Actor.ActorInfo.Health -= Math.Min(shortDamage, player.Actor.ActorInfo.Health);

					if (State.CurrentStateId == GameStateId.MatchRunning) {
						if (player.Actor.Cmid.CompareTo(peer.Actor.Cmid) != 0) {
							StatisticsManager.IncreaseWeaponShotsHit(peer, weapon.ItemClass);
							StatisticsManager.IncreaseWeaponDamageDone(peer, weapon.ItemClass, shortDamage);

						}

						StatisticsManager.IncreaseDamageReceived(player, shortDamage);
					}

					if (player.Actor.ActorInfo.Health <= 0) {
						player.Actor.ActorInfo.PlayerState |= PlayerStates.Dead;

						if (State.CurrentStateId == GameStateId.MatchRunning) {
							if (player.Actor.Cmid.CompareTo(peer.Actor.Cmid) != 0) {
								StatisticsManager.IncreaseDeaths(player);

								StatisticsManager.IncreaseWeaponKills(peer, weapon.ItemClass, (BodyPart)bodyPart);

								if (weapon.ItemClass == UberstrikeItemClass.WeaponSniperRifle) {
									StatisticsManager.IncreaseConsecutiveSnipes(peer);
								}

								var deltas = new List<GameActorInfoDelta> {
									player.Actor.Delta,
									peer.Actor.Delta
								};

								foreach (var _player in Players) {
									_player.GameEventSender.SendAllPlayerDeltas(deltas);
								}
							} else {
								StatisticsManager.IncreaseSuicides(peer);
							}
						}

						OnPlayerKilled(new PlayerKilledEventArgs {
							AttackerCmid = peer.Actor.Cmid,
							VictimCmid = player.Actor.Cmid,
							ItemClass = weapon.ItemClass,
							Damage = (ushort)shortDamage,
							Part = (BodyPart)bodyPart,
							Direction = -NormalizeVector(direction)
						});
					}
				}
			}
		}

		private void ExplosionDamage(GamePeer peer, int target, byte slot, byte distance, Vector3 force) {
			if (State.CurrentStateId != GameStateId.WaitingForPlayers && State.CurrentStateId != GameStateId.MatchRunning)
				return;
			if ((peer.Actor.ActorInfo.PlayerState & PlayerStates.Dead) != 0)
				return;

			foreach (var player in Players) {
				if (player.Actor.Cmid != target)
					continue;
				if ((player.Actor.ActorInfo.PlayerState & PlayerStates.Dead) != 0)
					return;
				if (Math.Abs((player.Actor.LastRespawnTime - DateTime.UtcNow).TotalSeconds) < 2)
					return;

				if (!GameServerApplication.Instance.Configuration.GameplaySettings.AllowFriendlyFire) {
					if (peer.Actor.ActorInfo.TeamID == player.Actor.ActorInfo.TeamID && peer.Actor.ActorInfo.TeamID != TeamID.NONE)
						return;
				}

				var weapon = peer.Actor.CurrentWeapon;
				if (weapon != null) {
					var damage = (weapon.DamagePerProjectile);

					var radius = weapon.SplashRadius / 100f;
					var damageExplosion = damage * (radius - distance) / radius;

					var shortDamage = (short)damageExplosion;

					var victimPos = player.Actor.Movement.Position;
					var attackerPos = peer.Actor.Movement.Position;

					var direction = attackerPos - victimPos;
					var back = new Vector3(0, 0, -1);

					var angle = Vector3.Angle(direction, back);
					if (direction.x < 0)
						angle = 360 - angle;

					var byteAngle = Conversion.Angle2Byte(angle);

					if (player.Actor.ActorInfo.ArmorPoints > 0) {
						int originalArmor = player.Actor.ActorInfo.ArmorPoints;
						player.Actor.ActorInfo.ArmorPoints = (byte)Math.Max(0, player.Actor.ActorInfo.ArmorPoints - shortDamage);

						double diff = (originalArmor - player.Actor.ActorInfo.ArmorPoints) * ARMOR_ABSORPTION;
						shortDamage -= (short)diff;
					}

					player.Actor.Damage.AddDamage(byteAngle, shortDamage, (byte)BodyPart.Body, 0, 0);
					player.Actor.ActorInfo.Health -= Math.Min(shortDamage, player.Actor.ActorInfo.Health);

					if (target != peer.Actor.Cmid) {
						player.GameEventSender.SendPlayerHit(force);
					}

					if (State.CurrentStateId == GameStateId.MatchRunning) {
						if (player.Actor.Cmid.CompareTo(peer.Actor.Cmid) != 0) {
							StatisticsManager.IncreaseWeaponShotsHit(peer, weapon.ItemClass);
							StatisticsManager.IncreaseWeaponDamageDone(peer, weapon.ItemClass, shortDamage);

						}

						StatisticsManager.IncreaseDamageReceived(player, shortDamage);
					}

					if (player.Actor.ActorInfo.Health <= 0) {
						player.Actor.ActorInfo.PlayerState |= PlayerStates.Dead;

						if (State.CurrentStateId == GameStateId.MatchRunning) {
							if (player.Actor.Cmid.CompareTo(peer.Actor.Cmid) != 0) {
								StatisticsManager.IncreaseDeaths(player);

								StatisticsManager.IncreaseWeaponKills(peer, weapon.ItemClass, BodyPart.Body);

								var deltas = new List<GameActorInfoDelta> {
									player.Actor.Delta,
									peer.Actor.Delta
								};

								foreach (var _player in Players) {
									_player.GameEventSender.SendAllPlayerDeltas(deltas);
								}
							} else {
								StatisticsManager.IncreaseSuicides(peer);
							}
						}

						OnPlayerKilled(new PlayerKilledEventArgs {
							AttackerCmid = peer.Actor.Cmid,
							VictimCmid = player.Actor.Cmid,
							ItemClass = weapon.ItemClass,
							Damage = (ushort)shortDamage,
							Part = BodyPart.Body,
							Direction = -NormalizeVector(direction)
						});
					}
				}
			}
		}

		private void DirectDamage(GamePeer peer, ushort damage) {
			// Not supported
		}

		private void DirectDeath(GamePeer peer) {
			peer.Actor.ActorInfo.PlayerState |= PlayerStates.Dead;

			peer.Actor.ActorInfo.Health = 0;
			peer.Actor.ActorInfo.ArmorPoints = 0;

			if (State.CurrentStateId == GameStateId.MatchRunning) {
				StatisticsManager.IncreaseSuicides(peer);
			}

			OnPlayerKilled(new PlayerKilledEventArgs {
				AttackerCmid = peer.Actor.Cmid,
				VictimCmid = peer.Actor.Cmid,
				ItemClass = UberstrikeItemClass.WeaponMachinegun,
				Part = BodyPart.Body,
				Direction = new Vector3()
			});
		}

		private void Jump(GamePeer peer, Vector3 position) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0)
					continue;

				otherPeer.GameEventSender.SendPlayerJumped(peer.Actor.Cmid, peer.Actor.Movement.Position);
			}
		}

		private void UpdatePositionAndRotation(GamePeer peer, ShortVector3 position, ShortVector3 velocity, byte hrot, byte vrot, byte moveState) {
			if (!peer.Actor.Movement.Position.Equals(position)) {
				peer.Actor.Movement.Position = position;
				peer.Actor.UpdatePosition = true;
			}

			if (!peer.Actor.Movement.Velocity.Equals(velocity)) {
				peer.Actor.Movement.Velocity = velocity;
				peer.Actor.UpdatePosition = true;
			}

			if (!peer.Actor.Movement.HorizontalRotation.Equals(hrot)) {
				peer.Actor.Movement.HorizontalRotation = hrot;
				peer.Actor.UpdatePosition = true;
			}

			if (!peer.Actor.Movement.VerticalRotation.Equals(vrot)) {
				peer.Actor.Movement.VerticalRotation = vrot;
				peer.Actor.UpdatePosition = true;
			}

			if (!peer.Actor.Movement.MovementState.Equals(moveState)) {
				peer.Actor.Movement.MovementState = moveState;
				peer.Actor.UpdatePosition = true;
			}
		}

		private void KickPlayer(GamePeer peer, int cmid) {
			if (peer.Actor.ActorInfo.AccessLevel < MemberAccessLevel.Moderator)
				return;

			FindPeerWithCmid(cmid)?.GameEventSender.SendKickPlayer("You have been removed from the game.");
		}

		private void IsFiring(GamePeer peer, bool on) {
			var state = peer.Actor.ActorInfo.PlayerState;

			if (on) {
				peer.Shooting.StartTime = SystemTime.Running;
				peer.Shooting.WeaponID = peer.Actor.ActorInfo.CurrentWeaponID;

				state |= PlayerStates.Shooting;
			} else {
				peer.Shooting.EndTime = SystemTime.Running;

				if (ShopManager.WeaponItems.TryGetValue(peer.Shooting.WeaponID, out var weapon)) {
					var shootTime = TimeSpan.FromMilliseconds(peer.Shooting.EndTime - peer.Shooting.StartTime);
					var shots = (int)Math.Ceiling(shootTime.TotalMilliseconds / weapon.RateOfFire);

					if (State.CurrentStateId == GameStateId.MatchRunning) {
						StatisticsManager.IncreaseWeaponShotsFired(peer, weapon.ItemClass, shots);
					}
				}

				peer.Shooting = new GamePeer.ShotData();

				state &= ~PlayerStates.Shooting;
			}

			peer.Actor.ActorInfo.PlayerState = state;
		}

		private void IsReadyForNextMatch(GamePeer peer, bool on) {
			// Not supported
		}

		private void IsPaused(GamePeer peer, bool on) {
			var state = peer.Actor.ActorInfo.PlayerState;

			if (on) {
				state |= PlayerStates.Paused;
			} else {
				state &= ~PlayerStates.Paused;
			}

			peer.Actor.ActorInfo.PlayerState = state;
		}

		private void IsInSniperMode(GamePeer peer, bool on) {
			var state = peer.Actor.ActorInfo.PlayerState;

			if (on) {
				state |= PlayerStates.Sniping;
			} else {
				state &= ~PlayerStates.Sniping;
			}

			peer.Actor.ActorInfo.PlayerState = state;
		}

		private void SingleBulletFire(GamePeer peer) {
			var weapon = peer.Actor.CurrentWeapon;

			if (weapon != null) {
				if (State.CurrentStateId == GameStateId.MatchRunning) {
					StatisticsManager.IncreaseWeaponShotsFired(peer, weapon.ItemClass, 1);
				}
			}

			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0)
					continue;

				otherPeer.GameEventSender.SendSingleBulletFire(peer.Actor.Cmid);
			}
		}

		private void SwitchWeapon(GamePeer peer, byte weaponSlot) {
			peer.Actor.ActorInfo.CurrentWeaponSlot = weaponSlot;

			var weapon = default(UberStrikeItemWeaponView);

			switch (weaponSlot) {
				case 0:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.MeleeWeapon, out weapon);
					break;
				case 1:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon1, out weapon);
					break;
				case 2:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon2, out weapon);
					break;
				case 3:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon3, out weapon);
					break;
				default:
					break;
			}

			peer.Actor.CurrentWeapon = weapon;
		}

		protected virtual void SwitchTeam(GamePeer peer) {
			// Implemented on a per-gamemode basis
		}

		private void ChangeGear(GamePeer peer, int head, int face, int upperBody, int lowerBody, int gloves, int boots, int holo) {
			// Not supported
		}

		private void EmitProjectile(GamePeer peer, Vector3 origin, Vector3 direction, byte slot, int projectileID, bool explode) {
			var shooterCmid = peer.Actor.Cmid;

			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != shooterCmid) {
					otherPeer.GameEventSender.SendEmitProjectile(shooterCmid, origin, direction, slot, projectileID, explode);
				}
			}
		}

		private void EmitQuickItem(GamePeer peer, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0)
					continue;

				otherPeer.GameEventSender.SendEmitQuickItem(origin, direction, itemId, playerNumber, projectileID);
			}
		}

		private void RemoveProjectile(GamePeer peer, int projectileId, bool explode) {
			foreach (var otherPeer in Peers) {
				otherPeer.GameEventSender.SendRemoveProjectile(projectileId, explode);
			}
		}

		private void HitFeedback(GamePeer peer, int targetCmid, Vector3 force) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid == targetCmid) {
					otherPeer.GameEventSender.SendPlayerHit(force);
					break;
				}
			}
		}

		private void ActivateQuickItem(GamePeer peer, QuickItemLogic logic, int robotLifeTime, int scrapsLifeTime, bool isInstant) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid.CompareTo(peer.Actor.Cmid) == 0)
					continue;

				otherPeer.GameEventSender.SendActivateQuickItem(peer.Actor.Cmid, logic, robotLifeTime, scrapsLifeTime, isInstant);
			}
		}

		private void ChatMessage(GamePeer peer, string message, byte context) {
			var actor = peer.Actor;

			var cmid = actor.Cmid;
			var playerName = actor.ActorInfo.PlayerName;
			var accessLevel = actor.ActorInfo.AccessLevel;

			var censored = ProfanityFilter.CensorString(message);
			var trimmed = censored.Substring(0, Math.Min(censored.Length, 140));

			if (GameServerApplication.Instance.Configuration.EnableChatLog) {
				ChatLog.Info($"[{MetaData.Number}] {playerName}: {message}");
			}

			GameServerApplication.Instance.SocketClient?.SendSync(PacketType.RoomChatMessage, (new SocketChatMessage {
				Cmid = peer.Actor.Cmid,
				Name = peer.Actor.Name,
				Message = trimmed
			}, MetaData));

			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != cmid) {
					otherPeer.GameEventSender.SendChatMessage(
						cmid,
						playerName,
						trimmed,
						accessLevel,
						context
					);
				}
			}
		}
		#endregion

		private Vector3 NormalizeVector(Vector3 vector) {
			float magnitude = vector.magnitude;
			if (magnitude > 1E-05f) {
				return vector / magnitude;
			}
			return new Vector3(0, 0, 0);
		}

		private void UpdateNextPlayerID() {
			var playerIds = Peers.Select(_ => _.Actor.ActorInfo.PlayerId).ToHashSet();

			NextPlayerId = Enumerable
				.Range(0, byte.MaxValue + 1)
				.Select(i => (byte)i)
				.FirstOrDefault(id => !playerIds.Contains(id));
		}
	}
}
