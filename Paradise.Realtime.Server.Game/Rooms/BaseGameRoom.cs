using log4net;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using Paradise.WebServices.Client;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public abstract partial class BaseGameRoom : BaseGameRoomOperationsHandler, IRoom<GamePeer>, IDisposable {
		private static readonly ILog Log = LogManager.GetLogger(typeof(BaseGameRoom));

		public int RoomId;
		public GameRoomData MetaData { get; private set; }

		private List<GamePeer> _peers = new List<GamePeer>();
		public IReadOnlyList<GamePeer> Peers => _peers.AsReadOnly();

		private List<GameActor> _players = new List<GameActor>();
		public IReadOnlyList<GameActor> Players => _players.AsReadOnly();

		private byte NextPlayerId;

		public Loop Loop { get; private set; }
		public ILoopScheduler Scheduler { get; private set; }

		public StateMachine<GameRoomState.Id> State { get; private set; }

		private bool IsDisposed = false;


		private string _password;
		public string Password {
			get { return _password; }
			set {
				MetaData.IsPasswordProtected = !string.IsNullOrEmpty(value);

				_password = value;
			}
		}


		public event EventHandler<EventArgs> MatchEnded;
		public event EventHandler<PlayerKilledEventArgs> PlayerKilled;
		public event EventHandler<PlayerRespawnedEventArgs> PlayerRespawned;
		public event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
		public event EventHandler<PlayerLeftEventArgs> PlayerLeft;


		public BaseGameRoom(GameRoomData metaData, ILoopScheduler scheduler) {
			MetaData = metaData ?? throw new ArgumentNullException(nameof(metaData));
			Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

			Loop = new Loop(OnTick, OnTickError);

			State = new StateMachine<GameRoomState.Id>();
			State.RegisterState(GameRoomState.Id.None, null);
			State.RegisterState(GameRoomState.Id.Debug, new DebugGameRoomState(this));
			State.RegisterState(GameRoomState.Id.WaitingForPlayers, new WaitingForPlayersGameRoomState(this));
			State.RegisterState(GameRoomState.Id.Countdown, new CountdownGameRoomState(this));
			State.RegisterState(GameRoomState.Id.Running, new RunningGameRoomState(this));

			State.SetState(GameRoomState.Id.WaitingForPlayers);

			Scheduler.Schedule(Loop);
		}

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
				SkinColor = Color.white, // Not trying to be racist here, that's what UberStrike wants ¯\_(ツ)_/¯
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
				otherPeer.Events.Game.SendPlayerLeftGame(peer.Actor.Cmid);
				//otherPeer.KnownActors.Remove(peer.Actor.Cmid);
			}

			//Actions.PlayerLeft(peer);

			lock (_peers) {
				_peers.Remove(peer);
				_players.Remove(peer.Actor);

				MetaData.ConnectedPlayers = Players.Count;
			}

			/* Set peer state to none, and clean up. */
			peer.State.SetState(GamePeerState.Id.None);
			peer.RemoveOperationHandler(Id);
			//peer.KnownActors.Clear();
			peer.Actor = null;
			peer.Room = null;
		}

		private void OnTick() {
			State.Update();
		}

		private void OnTickError(Exception e) {

			Log.Info("loop tick error", e);
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
				foreach (var peer in Peers) {
					foreach (var player in Players) {
						peer.Events.Game.SendPlayerLeftGame(player.Cmid);
					}
				}

				///* Clean up actors. */
				foreach (var peer in Peers) {
					peer.Actor = null;
					peer.RemoveOperationHandler(Id);

					peer.Disconnect();
					peer.Dispose();
				}

				///* Clear to lose refs to GameActor objects. */
				_peers.Clear();
				_players.Clear();
			}

			IsDisposed = true;
		}
		#region Events
		protected virtual void OnMatchEnded(EventArgs args) {
			MatchEnded?.Invoke(this, args);
		}

		protected virtual void OnPlayerRespawned(PlayerRespawnedEventArgs args) {
			PlayerRespawned?.Invoke(this, args);
		}

		protected virtual void OnPlayerJoined(PlayerJoinedEventArgs args) {
			Log.Info($"player joined: {args.Player}");
			PlayerJoined?.Invoke(this, args);
		}

		protected virtual void OnPlayerKilled(PlayerKilledEventArgs args) {
			PlayerKilled?.Invoke(this, args);
		}
		#endregion
	}
}
