using log4net;
using Paradise.Core.Models;
using Paradise.Core.Models.Views;
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
		public int RoomId {
			get { return MetaData.Number; }
			set { MetaData.Number = value; }
		}

		private List<GamePeer> _peers = new List<GamePeer>();
		public IReadOnlyList<GamePeer> Peers => _peers.AsReadOnly();

		private List<GamePeer> _players = new List<GamePeer>();
		public IReadOnlyList<GamePeer> Players => _players.AsReadOnly();

		public int RoundNumber;
		public int RoundStartTime;
		public int RoundEndTime;
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

		public TeamID WinningTeam = TeamID.NONE;

		public event EventHandler<EventArgs> MatchEnded;
		public event EventHandler<PlayerKilledEventArgs> PlayerKilled;
		public event EventHandler<PlayerRespawnedEventArgs> PlayerRespawned;
		public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
		public event EventHandler<PlayerLeftEventArgs> PlayerLeft;

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
			State.RegisterState(GameStateId.WaitingForPlayers, new WaitingForPlayersGameState(this));
			State.RegisterState(GameStateId.Countdown, new CountdownGameState(this));
			State.RegisterState(GameStateId.MatchRunning, new MatchRunningGameState(this));
			State.RegisterState(GameStateId.EndOfMatch, new EndOfMatchGameState(this));
			State.RegisterState(GameStateId.Debug, new DebugGameState(this));

			State.SetState(GameStateId.WaitingForPlayers);

			Scheduler.Schedule(Loop);
		}

		public abstract bool CanStartMatch { get; }

		public void Join(GamePeer peer) {
			if (peer == null) {
				throw new ArgumentNullException(nameof(peer));
			}

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

				// Not trying to be racist here, that's what UberStrike wants ¯\_(ツ)_/¯
				SkinColor = Color.white,

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

			byte armorPointCapacity = 0;
			foreach (var armor in actorInfo.Gear) {
				if (armor == 0) continue;

				var gear = default(UberStrikeItemGearView);
				if (ShopManager.GearItems.TryGetValue(armor, out gear)) {
					armorPointCapacity = (byte)Math.Min(200, armorPointCapacity + gear.ArmorPoints);
				}
			}

			actorInfo.ArmorPointCapacity = armorPointCapacity;
			actorInfo.ArmorPoints = actorInfo.ArmorPointCapacity;

			actorInfo.Weapons[0] = peer.Loadout.MeleeWeapon;
			actorInfo.Weapons[1] = peer.Loadout.Weapon1;
			actorInfo.Weapons[2] = peer.Loadout.Weapon2;
			actorInfo.Weapons[3] = peer.Loadout.Weapon3;

			if (actorInfo.Weapons[1] > 0) {
				actorInfo.CurrentWeaponSlot = 1;
			} else if (actorInfo.Weapons[2] > 0) {
				actorInfo.CurrentWeaponSlot = 2;
			} else if (actorInfo.Weapons[3] > 0) {
				actorInfo.CurrentWeaponSlot = 3;
			} else if (actorInfo.Weapons[0] > 0) {
				actorInfo.CurrentWeaponSlot = 0;
			}

			var number = -1;

			lock (_peers) {
				_peers.Add(peer);
				number = NextPlayerId++;
			}

			actorInfo.PlayerId = (byte)number;
			peer.Actor = new GameActor(actorInfo);
			peer.Actor.Number = number;
			peer.Room = this;

			peer.Actor.Delta.Changes.Clear();

			peer.AddOperationHandler(this);

			peer.PeerEvents.SendRoomEntered(MetaData);
			peer.State.SetState(PlayerStateId.Overview);

			MetaData.ConnectedPlayers = Peers.Count;
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

			peer.State.SetState(PlayerStateId.None);
			peer.RemoveOperationHandler(Id);
			peer.Actor = null;
			peer.Room = null;

			if (!CanStartMatch && (State.CurrentStateId == GameStateId.Countdown || State.CurrentStateId == GameStateId.MatchRunning) && !HasRoundEnded) {
				HasRoundEnded = true;
			}
		}

		public void Reset() {
			_frame = 6;
			frameTimer.Restart();

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
						delta.UpdateDeltaMask();
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

			if (HasRoundEnded && (State.CurrentStateId == GameStateId.Countdown || State.CurrentStateId == GameStateId.MatchRunning)) {
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
