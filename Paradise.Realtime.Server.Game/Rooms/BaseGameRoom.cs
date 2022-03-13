using log4net;
using Paradise.Core.Models;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public abstract partial class BaseGameRoom : BaseGameRoomOperationsHandler, IRoom<GamePeer>, IDisposable {
		private static readonly ILog Log = LogManager.GetLogger(typeof(BaseGameRoom));

		private bool IsDisposed = false;

		public GameRoomData MetaData { get; private set; }

		// May need to be split into players/actors for spectating
		private List<GamePeer> _peers = new List<GamePeer>();
		private List<GamePeer> _players = new List<GamePeer>();

		public IReadOnlyList<GamePeer> Peers => _peers.AsReadOnly();
		public IReadOnlyList<GamePeer> Players => _players.AsReadOnly();

		private byte _nextPlayer;

		public Loop Loop { get; private set; }
		public ILoopScheduler Scheduler { get; private set; }
		private ushort _frame;
		private readonly Timer _frameTimer;

		public StateMachine<GameRoomState.Id> State { get; private set; }

		public ShopManager ShopManager { get; private set; }
		public PowerUpManager PowerUpManager { get; private set; }
		public SpawnPointManager SpawnPointManager { get; private set; }

		private string _password;
		public string Password {
			get { return _password; }
			set {
				MetaData.IsPasswordProtected = !string.IsNullOrEmpty(value);

				_password = value;
			}
		}

		public int Number {
			get { return MetaData.Number; }
			set { MetaData.Number = value; }
		}

		public bool IsRunning => State.CurrentStateId == GameRoomState.Id.Running;
        public bool IsWaitingForPlayers => State.CurrentStateId == GameRoomState.Id.WaitingForPlayers;

		public int RoundNumber { get; set; }
		public int EndTime { get; set; }

		public event EventHandler<EventArgs> MatchEnded;
		public event EventHandler<PlayerKilledEventArgs> PlayerKilled;
		public event EventHandler<PlayerRespawnedEventArgs> PlayerRespawned;
		public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
		public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

		public BaseGameRoom(GameRoomData metaData, ILoopScheduler scheduler) {
			MetaData = metaData ?? throw new ArgumentNullException(nameof(metaData));
			Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

			Loop = new Loop(OnTick, OnTickError);

			ShopManager = new ShopManager();
			SpawnPointManager = new SpawnPointManager();
			PowerUpManager = new PowerUpManager(this);

			State = new StateMachine<GameRoomState.Id>();
			State.RegisterState(GameRoomState.Id.None, null);
			State.RegisterState(GameRoomState.Id.WaitingForPlayers, new WaitingForPlayersGameRoomState(this));
			State.RegisterState(GameRoomState.Id.Countdown, new CountdownGameRoomState(this));
			State.RegisterState(GameRoomState.Id.Running, new RunningGameRoomState(this));

			State.SetState(GameRoomState.Id.WaitingForPlayers);

			const float PARADISE_INTERVAL = 1000f / 9.5f;
			_frameTimer = new Timer(Loop, PARADISE_INTERVAL);

			Reset();

			Scheduler.Schedule(Loop);
		}

		private void OnTick() {
			bool updateMovements = _frameTimer.Tick();
            if (updateMovements) {
                _frame++;
			}

			State.Update();
		}

		private void OnTickError(Exception e) {
			Log.Info("loop tick error");
		}

		public virtual void Reset() {
			_frame = 6;
            _frameTimer.Restart();

            _nextPlayer = 0;

			_players.Clear();

            State.ResetState();
            State.SetState(GameRoomState.Id.WaitingForPlayers);

            Log.Info($"{this} has been reset.");
		}

		public void Join(GamePeer peer) {
			if (peer == null) {
                throw new ArgumentNullException(nameof(peer));
			}

			// peer.StatsPerLife = new List<StatsCollectionView>();
            // peer.TotalStats = new StatsCollectionView();
            // peer.CurrentLifeStats = new StatsCollectionView();
            // peer.WeaponStats = new Dictionary<int, WeaponStats>();
            // peer.Lifetimes = new List<TimeSpan>();

			peer.Loadout = new UserWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetLoadout(peer.AuthToken);

			var actorInfo = new GameActorInfo {
				TeamID = TeamID.NONE,
				Health = 100,
				Deaths = 0,
				Kills = 0,
				Level = 1,
				Channel = ChannelType.Steam,
				PlayerState = PlayerStates.None,
				Ping = (ushort)(peer.RoundTripTime / 2),
				Cmid = peer.Member.CmuneMemberView.PublicProfile.Cmid,
                ClanTag = peer.Member.CmuneMemberView.PublicProfile.GroupTag,
                AccessLevel = peer.Member.CmuneMemberView.PublicProfile.AccessLevel,
                PlayerName = peer.Member.CmuneMemberView.PublicProfile.Name,
			};

			actorInfo.Gear[0] = (int)peer.Loadout.Webbing; // Holo
            actorInfo.Gear[1] = peer.Loadout.Head;
            actorInfo.Gear[2] = peer.Loadout.Face;
            actorInfo.Gear[3] = peer.Loadout.Gloves;
            actorInfo.Gear[4] = peer.Loadout.UpperBody;
            actorInfo.Gear[5] = peer.Loadout.LowerBody;
            actorInfo.Gear[6] = peer.Loadout.Boots;

			actorInfo.Weapons[0] = peer.Loadout.MeleeWeapon;
            actorInfo.Weapons[1] = peer.Loadout.Weapon1;
            actorInfo.Weapons[2] = peer.Loadout.Weapon2;
            actorInfo.Weapons[3] = peer.Loadout.Weapon3;

			var number = 0;
            var actor = new GameActor(actorInfo);

            lock (_peers) {
                _peers.Add(peer);
                number = _nextPlayer++;
            }

			peer.Room = this;
            peer.Actor = actor;
            peer.Actor.Number = number;
            peer.LifeStart = DateTime.Now.TimeOfDay;
            peer.AddOperationHandler(this);

			peer.Events.SendRoomEntered(MetaData);

			peer.State.SetState(GamePeerState.Id.Overview);
		}

		public void Leave(GamePeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

			System.Diagnostics.Debug.Assert(peer.Room != null, "GamePeer is leaving room, but is not in any room.");
			System.Diagnostics.Debug.Assert(peer.Room == this, "GamePeer is leaving room, but is not leaving the correct room.");

			/* Let other peers know that the peer has left the room. */
			
            foreach (var otherPeer in Peers) {
                otherPeer.Events.Room.SendPlayerLeftGame(peer.Actor.Cmid);
                //otherPeer.KnownActors.Remove(peer.Actor.Cmid);
            }
            
			//Actions.PlayerLeft(peer);

			lock (_peers) {
				_peers.Remove(peer);
				_players.Remove(peer);

				MetaData.ConnectedPlayers = Players.Count;
			}

			/* Set peer state to none, and clean up. */
			peer.State.SetState(GamePeerState.Id.None);
			peer.RemoveOperationHandler(Id);
			peer.KnownActors.Clear();
			peer.Actor = null;
			peer.Room = null;
		}



		public override string ToString() {
            return $"(room \"{MetaData.Name}\":{Number} {MetaData.ConnectedPlayers}/{MetaData.PlayerLimit} state {State.CurrentStateId})";
        }

		public void Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				Scheduler.Unschedule(Loop);

				///* Best effort clean up. */
				//foreach (var player in Players) {
				//	foreach (var otherActor in Actors)
				//		otherActor.Peer.Events.Game.SendPlayerLeftGame(player.Cmid);
				//}

				///* Clean up actors. */
				foreach (var peer in Peers) {
					//	var peer = actor.Peer;
					//peer.Actor = null;
					//peer.Handlers.Remove(Id);

					peer.Disconnect();
					peer.Dispose();
				}

				///* Clear to lose refs to GameActor objects. */
				//_actors.Clear();
				_players.Clear();
			}

			IsDisposed = true;
		}

		#region Operations
		protected override void OnActivateQuickItem(GamePeer peer, QuickItemLogic logic, int robotLifeTime, int scrapsLifeTime, bool isInstant) {
			throw new NotImplementedException();
		}

		protected override void OnChangeGear(GamePeer peer, int head, int face, int upperBody, int lowerBody, int gloves, int boots, int holo) {
			throw new NotImplementedException();
		}

		protected override void OnChatMessage(GamePeer peer, string message, byte context) {
			throw new NotImplementedException();
		}

		protected override void OnDirectDamage(GamePeer peer, ushort damage) {
			throw new NotImplementedException();
		}

		protected override void OnDirectDeath(GamePeer peer) {
			OnPlayerKilled(new PlayerKilledEventArgs {
				AttackerCmid = peer.Actor.Cmid,
				VictimCmid = peer.Actor.Cmid,
				ItemClass = UberstrikeItemClass.WeaponMachinegun,
				Part = BodyPart.Body,
				Direction = new Vector3()
			});
		}

		protected override void OnDirectHitDamage(GamePeer peer, int target, byte bodyPart, byte bullets) {
			
		}

		protected override void OnEmitProjectile(GamePeer peer, Vector3 origin, Vector3 direction, byte slot, int projectileID, bool explode) {
			throw new NotImplementedException();
		}

		protected override void OnEmitQuickItem(GamePeer peer, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
			throw new NotImplementedException();
		}

		protected override void OnExplosionDamage(GamePeer peer, int target, byte slot, byte distance, Vector3 force) {
			throw new NotImplementedException();
		}

		protected override void OnHitFeedback(GamePeer peer, int targetCmid, Vector3 force) {
			throw new NotImplementedException();
		}

		protected override void OnIncreaseHealthAndArmor(GamePeer peer, byte health, byte armor) {
			throw new NotImplementedException();
		}

		protected override void OnIsFiring(GamePeer peer, bool on) {
			var state = peer.Actor.Info.PlayerState;
			if (on) {
				state |= PlayerStates.Shooting;
			} else {
				state &= ~PlayerStates.Shooting;
			}

			peer.Actor.Info.PlayerState = state;
		}

		protected override void OnIsInSniperMode(GamePeer peer, bool on) {
			var state = peer.Actor.Info.PlayerState;
			if (on) {
				state |= PlayerStates.Sniping;
			} else {
				state &= ~PlayerStates.Sniping;
			}

			peer.Actor.Info.PlayerState = state;
		}

		protected override void OnIsPaused(GamePeer peer, bool on) {
			var state = peer.Actor.Info.PlayerState;
			if (on) {
				state |= PlayerStates.Paused;
			} else {
				state &= ~PlayerStates.Paused;
			}

			peer.Actor.Info.PlayerState = state;
		}

		protected override void OnIsReadyForNextMatch(GamePeer peer, bool on) {
			throw new NotImplementedException();
		}

		protected override void OnJoinAsSpectator(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnJoinGame(GamePeer peer, TeamID team) {
			peer.Actor.Team = team;
			peer.Actor.Info.Health = 100;
			peer.Actor.Info.Ping = (ushort)(peer.RoundTripTime / 2);
			peer.Actor.Info.PlayerState = PlayerStates.Ready;
			//peer.Loadout = peer.Web.GetLoadout();
			Log.Info("Joining game in progress.");

			lock (_peers) {
				if (_players.FirstOrDefault(_ => _.Actor.Cmid == peer.Actor.Cmid) == null) {
					_players.Add(peer);

					MetaData.ConnectedPlayers = Players.Count;
				}
			}

			OnPlayerJoined(new PlayerJoinedEventArgs {
				Player = peer,
				Team = team
			});

			Log.Info($"Joining team -> CMID:{peer.Actor.Cmid}:{team}:{peer.Actor.Number}");
		}

		protected override void OnJump(GamePeer peer, Vector3 position) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != peer.Actor.Cmid) {
					otherPeer.Events.Room.SendPlayerJumped(peer.Actor.Cmid, peer.Actor.Movement.Position);
				}
			}
		}

		protected override void OnKickPlayer(GamePeer peer, int cmid) {
			throw new NotImplementedException();
		}

		protected override void OnOpenDoor(GamePeer peer, int doorId) {
			throw new NotImplementedException();
		}

		protected override void OnPowerUpPicked(GamePeer peer, int powerupId, byte type, byte value) {
			throw new NotImplementedException();
		}

		protected override void OnPowerUpRespawnTimes(GamePeer peer, List<ushort> respawnTimes) {
			if (!PowerUpManager.IsLoaded) {
				PowerUpManager.Load(respawnTimes);
			}
		}

		protected override void OnRemoveProjectile(GamePeer peer, int projectileID, bool explode) {
			throw new NotImplementedException();
		}

		protected override void OnRespawnRequest(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnSingleBulletFire(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnSpawnPositions(GamePeer peer, TeamID team, List<Vector3> positions, List<byte> rotations) {
			System.Diagnostics.Debug.Assert(positions.Count == rotations.Count, "Number of spawn positions given and number of rotations given is not equal.");

			if (!SpawnPointManager.IsLoaded(team)) {
				SpawnPointManager.Load(team, positions, rotations);
			}
		}

		protected override void OnSwitchTeam(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnSwitchWeapon(GamePeer peer, byte weaponSlot) {
			//peer.Actor.Info.ShootingTick = 0;
			peer.Actor.Info.CurrentWeaponSlot = weaponSlot;
		}

		protected override void OnUpdatePositionAndRotation(GamePeer peer, ShortVector3 position, ShortVector3 velocity, byte hrot, byte vrot, byte moveState) {
			peer.Actor.Movement.Position = position;
			peer.Actor.Movement.Velocity = velocity;
			peer.Actor.Movement.HorizontalRotation = hrot;
			peer.Actor.Movement.VerticalRotation = vrot;
			peer.Actor.Movement.MovementState = moveState;
		}
		#endregion

		#region Events
		protected virtual void OnMatchEnded(EventArgs e) {
			MatchEnded?.Invoke(this, e);
			//List<GamePeer> playersToRemove = new List<GamePeer>();

			//foreach (var i in _players) {
			//	foreach (var x in _players) {
			//		if (x.Actor.Cmid == i.Actor.Cmid)
			//			continue;
			//		x.Events.Game.SendPlayerLeftGame(i.Actor.Cmid);
			//	}
			//	playersToRemove.Add(i);
			//}
			//foreach (var i in playersToRemove) {
			//	_players.Remove(i);
			//}
			//_view.ConnectedPlayers = Players.Count;
		}

		protected virtual void OnPlayerRespawned(PlayerRespawnedEventArgs args) {
			Log.Debug($"OnPlayerRespawned invoked for {args.Player.Actor.PlayerName}.");
			args.Player.LifeStart = DateTime.Now.TimeOfDay;
			PlayerRespawned?.Invoke(this, args);
		}

		protected virtual void OnPlayerJoined(PlayerJoinedEventArgs args) {
			PlayerJoined?.Invoke(this, args);
		}

		protected virtual void OnPlayerKilled(PlayerKilledEventArgs args) {
			PlayerKilled?.Invoke(this, args);

			//foreach (var player in Players) {
			//	if (player.Actor.Cmid == args.AttackerCmid) {
			//		bool flag = DateTime.Now.TimeOfDay < player.lastKillTime.Add(TimeSpan.FromSeconds(10));
			//		player.killCounter = ((!flag) ? 1 : (player.killCounter + 1));
			//		player.lastKillTime = DateTime.Now.TimeOfDay;
			//		if (player.killCounter > player.CurrentLifeStats.ConsecutiveSnipes)
			//			player.CurrentLifeStats.ConsecutiveSnipes = player.killCounter;
			//		if (player.killCounter > player.TotalStats.ConsecutiveSnipes)
			//			player.TotalStats.ConsecutiveSnipes = player.killCounter;
			//	} else if (player.Actor.Cmid == args.VictimCmid) {
			//		player.TotalStats.Deaths++;
			//		player.LifeEnd = DateTime.Now.TimeOfDay;
			//		player.StatsPerLife.Add(player.CurrentLifeStats);
			//		player.Lifetimes.Add(player.LifeEnd - player.LifeStart);
			//		player.CurrentLifeStats = new StatsCollectionView();
			//	}
			//}
		}
		#endregion
	}
}