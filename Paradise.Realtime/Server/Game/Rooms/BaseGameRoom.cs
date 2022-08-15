using log4net;
using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public enum GAME_FLAGS {
		None = 0,
		LowGravity = 1,
		NoArmor = 2,
		QuickSwitch = 4,
		MeleeOnly = 8
	}

	public abstract partial class BaseGameRoom : BaseGameRoomOperationsHandler, IRoom<GamePeer>, IDisposable {
		private static readonly ILog Log = LogManager.GetLogger(typeof(BaseGameRoom));

		private bool IsDisposed = false;

		public GameRoomData MetaData { get; private set; }
		public int RoomId {
			get { return MetaData.Number; }
			set { MetaData.Number = value; }
		}

		public bool IsTeamGame {
			get {
				return MetaData.GameMode == GameModeType.TeamDeathMatch || MetaData.GameMode == GameModeType.EliminationMode;
			}
		}

		private List<GamePeer> _peers = new List<GamePeer>();
		public IReadOnlyList<GamePeer> Peers => _peers.AsReadOnly();

		private List<GamePeer> _players = new List<GamePeer>();
		public IReadOnlyList<GamePeer> Players => _players.AsReadOnly();

		public int RoundNumber;
		public int RoundStartTime;
		public int RoundEndTime;
		public List<TimeSpan> RoundDurations = new List<TimeSpan>();
		public bool HasRoundEnded;

		private byte NextPlayerId;

		public Loop Loop { get; private set; }
		public ILoopScheduler Scheduler { get; private set; }

		private Timer frameTimer;
		private ushort _frame;

		public StateMachine<GameStateId> State { get; private set; }

		public SpawnPointManager SpawnPointManager { get; private set; }
		public ShopManager ShopManager { get; private set; }
		public PowerUpManager PowerUpManager { get; private set; }

		private string _password;
		public string Password {
			get { return _password; }
			set {
				MetaData.IsPasswordProtected = !string.IsNullOrEmpty(value);

				_password = value;
			}
		}

		public int WinningCmid = 0;
		public TeamID WinningTeam = TeamID.NONE;

		public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
		public event EventHandler<PlayerLeftEventArgs> PlayerLeft;
		public event EventHandler<PlayerKilledEventArgs> PlayerKilled;
		public event EventHandler<PlayerRespawnedEventArgs> PlayerRespawned;
		public event EventHandler<EventArgs> MatchStarted;
		public event EventHandler<EventArgs> MatchEnded;

		public BaseGameRoom(GameRoomData metaData, ILoopScheduler scheduler) {
			MetaData = metaData ?? throw new ArgumentNullException(nameof(metaData));
			Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

			SpawnPointManager = new SpawnPointManager();
			ShopManager = new ShopManager();
			PowerUpManager = new PowerUpManager(this);

			ShopManager.Load();

			Loop = new Loop(OnTick, OnTickError);

			_frame = 6;
			frameTimer = new Timer(Loop, 1000 / 9.5f);
			frameTimer.Restart();

			State = new StateMachine<GameStateId>();
			State.RegisterState(GameStateId.None, null);
			State.RegisterState(GameStateId.WaitingForPlayers, new WaitingForPlayersState(this));
			State.RegisterState(GameStateId.PrepareNextRound, new PrepareNextRoundState(this));
			State.RegisterState(GameStateId.MatchRunning, new MatchRunningState(this));
			State.RegisterState(GameStateId.EndOfMatch, new EndOfMatchState(this));
			State.RegisterState(GameStateId.AfterRound, new AfterRoundState(this));

			State.SetState(GameStateId.WaitingForPlayers);

			Scheduler.Schedule(Loop);
		}

		public abstract bool CanJoinMatch { get; }
		public abstract bool CanStartMatch { get; }
		public abstract void GetCurrentScore(out short killsRemaining, out short blueTeamScore, out short redTeamScore);

		public void Join(GamePeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			peer.PreviousSpawnPoints.Clear();
			var actorInfo = new GameActorInfo {
				TeamID = TeamID.NONE,
				Health = 100,
				Level = XpPointsUtil.GetLevelForXp(peer.Member.UberstrikeMemberView.PlayerStatisticsView.Xp),
				Channel = ChannelType.Steam,
				PlayerState = PlayerStates.None,
				Ping = (ushort)(peer.RoundTripTime / 2),

				// Not trying to be racist here, that's what UberStrike wants ¯\_(ツ)_/¯
				SkinColor = Color.white,

				Cmid = peer.Member.CmuneMemberView.PublicProfile.Cmid,
				ClanTag = peer.Member.CmuneMemberView.PublicProfile.GroupTag,
				AccessLevel = peer.Member.CmuneMemberView.PublicProfile.AccessLevel,
				PlayerName = peer.Member.CmuneMemberView.PublicProfile.Name,
			};

			var number = -1;

			lock (_peers) {
				_peers.Add(peer);
				number = NextPlayerId++;
			}

			actorInfo.PlayerId = (byte)number;
			peer.Actor = new GameActor(actorInfo);
			peer.Actor.Peer = peer;
			peer.Actor.Number = number;
			peer.Room = this;

			peer.Actor.Delta.Changes.Clear();

			peer.AddOperationHandler(this);

			peer.PeerEvents.SendRoomEntered(MetaData);
			peer.State.SetState(PlayerStateId.Overview);

			MetaData.ConnectedPlayers = Peers.Count;

			Log.Info($"{peer.Actor.PlayerName}({peer.Actor.Cmid}) joined {this}({this.Id})");
		}

		public void Leave(GamePeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			System.Diagnostics.Debug.Assert(peer.Room != null, "GamePeer is leaving room, but is not in any room.");
			System.Diagnostics.Debug.Assert(peer.Room == this, "GamePeer is leaving room, but is not leaving the correct room.");

			foreach (var otherPeer in Peers) {
				otherPeer.GameEvents.SendPlayerLeftGame(peer.Actor.Cmid);
			}

			lock (_peers) {
				_peers.Remove(peer);
				_players.Remove(peer);

				MetaData.ConnectedPlayers = Peers.Count;
			}

			Log.Info($"{peer.Actor.PlayerName}({peer.Actor.Cmid}) left {this}({this.Id})");

			peer.State.SetState(PlayerStateId.None);
			peer.RemoveOperationHandler(Id);
			peer.Actor = null;
			peer.Room = null;

			OnPlayerLeft(new PlayerLeftEventArgs {
				Player = peer
			});
		}

		public virtual void Reset() {
			_frame = 6;
			frameTimer.Restart();

			RoundNumber = 0;
			RoundStartTime = int.MinValue;
			RoundEndTime = int.MinValue;
			RoundDurations.Clear();


			NextPlayerId = 0;

			foreach (var peer in Peers) {
				foreach (var otherPeer in Peers) {
					otherPeer.GameEvents.SendPlayerLeftGame(peer.Actor.Cmid);
				}
			}

			_players.Clear();

			State.ResetState();
			State.SetState(GameStateId.WaitingForPlayers);
		}

		private void OnTick() {
			var updatePositions = frameTimer.Tick();
			if (updatePositions) _frame++;

			State.Update();

			var positions = new List<PlayerMovement>();
			var deltas = new List<GameActorInfoDelta>();

			foreach (var peer in Peers) {
				if (peer.HasError) {
					peer.Disconnect();

					continue;
				}

				peer.Tick();
				peer.State.Update();

				var actor = peer.Actor;

				if (Players.Contains(peer)) {
					var delta = actor.Delta;

					if (delta.Changes.Count > 0) {
						deltas.Add(delta);
					}

					if (actor.Damage.Count > 0) {
						peer.GameEvents.SendDamageEvent(actor.Damage);
						actor.Damage.Clear();
					}

					if (updatePositions && actor.Info.IsAlive) {
						positions.Add(actor.Movement);
					}
				}
			}

			if (deltas.Count > 0) {
				foreach (var peer in Peers) {
					peer.GameEvents.SendAllPlayerDeltas(deltas);
				}

				foreach (var delta in deltas) {
					delta.Reset();
				}

				deltas.Clear();
			}

			if (positions.Count > 0 && updatePositions) {
				foreach (var peer in Peers) {
					peer.GameEvents.SendAllPlayerPositions(positions, _frame);
				}

				positions.Clear();
			}

			if (HasRoundEnded && (State.CurrentStateId == GameStateId.PrepareNextRound || State.CurrentStateId == GameStateId.MatchRunning)) {
				OnMatchEnded(new EventArgs());
			}
		}

		private void OnTickError(Exception e) {
			Log.Info("loop tick error", e);
		}

		public void Dispose() {
			Dispose(true);
		}

		private void Dispose(bool disposing) {
			if (IsDisposed) return;

			if (disposing) {
				Scheduler.Unschedule(Loop);

				foreach (var peer in Peers) {
					foreach (var player in Players) {
						peer.GameEvents.SendPlayerLeftGame(player.Actor.Cmid);
					}
				}

				foreach (var peer in Peers) {
					peer.Actor = null;
					peer.RemoveOperationHandler(Id);

					peer.Disconnect();
					peer.Dispose();
				}

				_peers.Clear();
				_players.Clear();
			}

			IsDisposed = true;
		}

		#region Player Management
		public void PreparePlayer(GamePeer player) {
			player.Actor.Info.Kills = 0;
			player.Actor.Info.Deaths = 0;

			player.Loadout = new UserWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetLoadout(player.AuthToken);

			if (((GAME_FLAGS)MetaData.GameFlags & GAME_FLAGS.NoArmor) == 0) {
				player.Actor.Info.Gear[0] = (int)player.Loadout.Webbing; // Holo
				player.Actor.Info.Gear[1] = player.Loadout.Head;
				player.Actor.Info.Gear[2] = player.Loadout.Face;
				player.Actor.Info.Gear[3] = player.Loadout.Gloves;
				player.Actor.Info.Gear[4] = player.Loadout.UpperBody;
				player.Actor.Info.Gear[5] = player.Loadout.LowerBody;
				player.Actor.Info.Gear[6] = player.Loadout.Boots;
			}

			player.Actor.Info.Health = 100;

			byte armorPointCapacity = 0;
			foreach (var armor in player.Actor.Info.Gear) {
				if (armor == 0) continue;

				var gear = default(UberStrikeItemGearView);
				if (ShopManager.GearItems.TryGetValue(armor, out gear)) {
					armorPointCapacity = (byte)Math.Min(200, armorPointCapacity + gear.ArmorPoints);
				}
			}

			player.Actor.Info.ArmorPointCapacity = armorPointCapacity;
			player.Actor.Info.ArmorPoints = player.Actor.Info.ArmorPointCapacity;

			player.Actor.Info.Weapons[0] = player.Loadout.MeleeWeapon;

			if (((GAME_FLAGS)MetaData.GameFlags & GAME_FLAGS.MeleeOnly) == 0) {
				player.Actor.Info.Weapons[1] = player.Loadout.Weapon1;
				player.Actor.Info.Weapons[2] = player.Loadout.Weapon2;
				player.Actor.Info.Weapons[3] = player.Loadout.Weapon3;
			}

			if (player.Actor.Info.Weapons[1] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 1;
			} else if (player.Actor.Info.Weapons[2] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 2;
			} else if (player.Actor.Info.Weapons[3] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 3;
			} else if (player.Actor.Info.Weapons[0] > 0) {
				player.Actor.Info.CurrentWeaponSlot = 0;
			}
		}

		public void SpawnPlayer(GamePeer player, bool joined = false) {
			var spawn = SpawnPointManager.Get(player.Actor.Team);

			if (joined) {
				if (!SpawnPointManager.SpawnPointsInUse.ContainsKey(player.Actor.Team)) {
					SpawnPointManager.SpawnPointsInUse[player.Actor.Team] = new List<SpawnPoint> { spawn };
				} else {
					while (SpawnPointManager.SpawnPointsInUse[player.Actor.Team].Contains(spawn)) {
						spawn = SpawnPointManager.Get(player.Actor.Team);
					}

					SpawnPointManager.SpawnPointsInUse[player.Actor.Team].Add(spawn);
				}
			} else {
				player.Actor.Info.Health = 100;
				player.Actor.Info.ArmorPoints = player.Actor.Info.ArmorPointCapacity;
				player.Actor.Info.PlayerState = PlayerStates.None;

				if (player.PreviousSpawnPoints.Count >= SpawnPointManager.GetSpawnPointCount(player.Actor.Team)) {
					player.PreviousSpawnPoints.Clear();
				}

				while (player.PreviousSpawnPoints.Contains(spawn)) {
					spawn = SpawnPointManager.Get(player.Actor.Team);
				}
			}

			player.PreviousSpawnPoints.Add(spawn);

			player.Actor.Movement.Position = spawn.Position;
			player.Actor.Movement.HorizontalRotation = spawn.Rotation;

			foreach (var peer in Peers) {
				if (joined && !peer.Actor.Info.IsSpectator) {
					peer.GameEvents.SendPlayerJoinedGame(player.Actor.Info, player.Actor.Movement);
				} else {
					peer.GameEvents.SendPlayerRespawned(player.Actor.Info.Cmid, player.Actor.Movement.Position, player.Actor.Movement.HorizontalRotation);
				}
			}
		}
		#endregion

		#region Events
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
	}
}
