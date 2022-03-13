using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public class GameRoomEvents : BaseEventSender {
		private static readonly ILog Log = LogManager.GetLogger(typeof(GameRoomEvents));

		public GameRoomEvents(BasePeer peer) : base(peer) { }

		public void SendPowerUpPicked(int id, byte flag) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, id);
				ByteProxy.Serialize(bytes, flag);

				SendEvent((byte)IGameRoomEventsType.PowerUpPicked, bytes);
			}
		}

		public void SendSetPowerupState(List<int> states) {
			using (var bytes = new MemoryStream()) {
				ListProxy<int>.Serialize(bytes, states, Int32Proxy.Serialize);

				SendEvent((byte)IGameRoomEventsType.SetPowerupState, bytes);
			}
		}

		public void SendResetAllPowerups() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGameRoomEventsType.ResetAllPowerups, bytes);
			}
		}

		public void SendDoorOpen(int id) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, id);

				SendEvent((byte)IGameRoomEventsType.DoorOpen, bytes);
			}
		}

		public void SendDisconnectCountdown(byte countdown) {
			using (var bytes = new MemoryStream()) {
				ByteProxy.Serialize(bytes, countdown);

				SendEvent((byte)IGameRoomEventsType.DisconnectCountdown, bytes);
			}
		}

		public void SendMatchStartCountdown(byte countdown) {
			using (var bytes = new MemoryStream()) {
				ByteProxy.Serialize(bytes, countdown);

				SendEvent((byte)IGameRoomEventsType.MatchStartCountdown, bytes);
			}
		}

		public void SendMatchStart(int roundNumber, int endTime) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, roundNumber);
				Int32Proxy.Serialize(bytes, endTime);

				SendEvent((byte)IGameRoomEventsType.MatchStart, bytes);
			}
		}

		public void SendMatchEnd(EndOfMatchData data) {
			using (var bytes = new MemoryStream()) {
				EndOfMatchDataProxy.Serialize(bytes, data);

				SendEvent((byte)IGameRoomEventsType.MatchEnd, bytes);
			}
		}

		public void SendTeamWins(TeamID team) {
			using (var bytes = new MemoryStream()) {
				EnumProxy<TeamID>.Serialize(bytes, team);

				SendEvent((byte)IGameRoomEventsType.TeamWins, bytes);
			}
		}

		public void SendWaitingForPlayers() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGameRoomEventsType.WaitingForPlayers, bytes);
			}
		}

		public void SendPrepareNextRound() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGameRoomEventsType.PrepareNextRound, bytes);
			}
		}

		public void SendAllPlayers(List<GameActorInfo> allPlayers, List<PlayerMovement> allPositions, ushort gameframe) {
			Log.Info($"send all players to {Peer}");
			using (var bytes = new MemoryStream()) {
				ListProxy<GameActorInfo>.Serialize(bytes, allPlayers, GameActorInfoProxy.Serialize);
				ListProxy<PlayerMovement>.Serialize(bytes, allPositions, PlayerMovementProxy.Serialize);
				UInt16Proxy.Serialize(bytes, gameframe);

				SendEvent((byte)IGameRoomEventsType.AllPlayers, bytes);
			}
		}

		public void SendAllPlayerDeltas(List<GameActorInfoDelta> allDeltas) {
			using (var bytes = new MemoryStream()) {
				ListProxy<GameActorInfoDelta>.Serialize(bytes, allDeltas, GameActorInfoDeltaProxy.Serialize);

				SendEvent((byte)IGameRoomEventsType.AllPlayerDeltas, bytes);
			}
		}

		public void SendAllPlayerPositions(List<PlayerMovement> allPositions, ushort gameframe) {
			using (var bytes = new MemoryStream()) {
				ListProxy<PlayerMovement>.Serialize(bytes, allPositions, PlayerMovementProxy.Serialize);
				UInt16Proxy.Serialize(bytes, gameframe);

				SendEvent((byte)IGameRoomEventsType.AllPlayerPositions, bytes);
			}
		}

		public void SendPlayerDelta(GameActorInfoDelta delta) {
			using (var bytes = new MemoryStream()) {
				GameActorInfoDeltaProxy.Serialize(bytes, delta);

				SendEvent((byte)IGameRoomEventsType.PlayerDelta, bytes);
			}
		}

		public void SendPlayerJumped(int cmid, Vector3 position) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				Vector3Proxy.Serialize(bytes, position);

				SendEvent((byte)IGameRoomEventsType.PlayerJumped, bytes);
			}
		}

		public void SendPlayerRespawnCountdown(byte countdown) {
			using (var bytes = new MemoryStream()) {
				ByteProxy.Serialize(bytes, countdown);

				SendEvent((byte)IGameRoomEventsType.PlayerRespawnCountdown, bytes);
			}
		}

		public void SendPlayerRespawned(int cmid, Vector3 position, byte rotation) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				Vector3Proxy.Serialize(bytes, position);
				ByteProxy.Serialize(bytes, rotation);

				SendEvent((byte)IGameRoomEventsType.PlayerRespawned, bytes);
			}
		}

		public void SendPlayerJoinedGame(GameActorInfo player, PlayerMovement position) {
			using (var bytes = new MemoryStream()) {
				GameActorInfoProxy.Serialize(bytes, player);
				PlayerMovementProxy.Serialize(bytes, position);

				SendEvent((byte)IGameRoomEventsType.PlayerJoinedGame, bytes);
			}
		}

		public void SendJoinGameFailed(string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)IGameRoomEventsType.JoinGameFailed, bytes);
			}
		}

		public void SendPlayerLeftGame(int cmid) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);

				SendEvent((byte)IGameRoomEventsType.PlayerLeftGame, bytes);
			}
		}

		public void SendPlayerChangedTeam(int cmid, TeamID team) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				EnumProxy<TeamID>.Serialize(bytes, team);

				SendEvent((byte)IGameRoomEventsType.PlayerChangedTeam, bytes);
			}
		}

		public void SendJoinedAsSpectator() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGameRoomEventsType.JoinedAsSpectator, bytes);
			}
		}

		public void SendPlayersReadyUpdated() {
			using (var bytes = new MemoryStream()) {
				SendEvent((byte)IGameRoomEventsType.PlayersReadyUpdated, bytes);
			}
		}

		public void SendDamageEvent(DamageEvent damageEvent) {
			using (var bytes = new MemoryStream()) {
				DamageEventProxy.Serialize(bytes, damageEvent);

				SendEvent((byte)IGameRoomEventsType.DamageEvent, bytes);
			}
		}

		public void SendPlayerKilled(int shooter, int target, byte weaponClass, ushort damage, byte bodyPart, Vector3 direction) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, shooter);
				Int32Proxy.Serialize(bytes, target);
				ByteProxy.Serialize(bytes, weaponClass);
				UInt16Proxy.Serialize(bytes, damage);
				ByteProxy.Serialize(bytes, bodyPart);
				Vector3Proxy.Serialize(bytes, direction);

				SendEvent((byte)IGameRoomEventsType.PlayerKilled, bytes);
			}
		}

		public void SendUpdateRoundScore(int round, short blue, short red) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, round);
				Int16Proxy.Serialize(bytes, blue);
				Int16Proxy.Serialize(bytes, red);

				SendEvent((byte)IGameRoomEventsType.UpdateRoundScore, bytes);
			}
		}

		public void SendKillsRemaining(int killsRemaining, int leaderCmid) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, killsRemaining);
				Int32Proxy.Serialize(bytes, leaderCmid);

				SendEvent((byte)IGameRoomEventsType.KillsRemaining, bytes);
			}
		}

		public void SendLevelUp(int newLevel) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, newLevel);

				SendEvent((byte)IGameRoomEventsType.LevelUp, bytes);
			}
		}

		public void SendKickPlayer(string message) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, message);

				SendEvent((byte)IGameRoomEventsType.KickPlayer, bytes);
			}
		}

		public void SendQuickItemEvent(int cmid, byte eventType, int robotLifeTime, int scrapsLifeTime, bool isInstant) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				ByteProxy.Serialize(bytes, eventType);
				Int32Proxy.Serialize(bytes, robotLifeTime);
				Int32Proxy.Serialize(bytes, scrapsLifeTime);
				BooleanProxy.Serialize(bytes, isInstant);

				SendEvent((byte)IGameRoomEventsType.QuickItemEvent, bytes);
			}
		}

		public void SendSingleBulletFire(int cmid) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);

				SendEvent((byte)IGameRoomEventsType.SingleBulletFire, bytes);
			}
		}

		public void SendPlayerHit(Vector3 force) {
			using (var bytes = new MemoryStream()) {
				Vector3Proxy.Serialize(bytes, force);

				SendEvent((byte)IGameRoomEventsType.PlayerHit, bytes);
			}
		}

		public void SendRemoveProjectile(int projectileId, bool explode) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, projectileId);
				BooleanProxy.Serialize(bytes, explode);

				SendEvent((byte)IGameRoomEventsType.RemoveProjectile, bytes);
			}
		}

		public void SendEmitProjectile(int cmid, Vector3 origin, Vector3 direction, byte slot, int projectileID, bool explode) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				Vector3Proxy.Serialize(bytes, origin);
				Vector3Proxy.Serialize(bytes, direction);
				ByteProxy.Serialize(bytes, slot);
				Int32Proxy.Serialize(bytes, projectileID);
				BooleanProxy.Serialize(bytes, explode);

				SendEvent((byte)IGameRoomEventsType.EmitProjectile, bytes);
			}
		}

		public void SendEmitQuickItem(Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
			using (var bytes = new MemoryStream()) {
				Vector3Proxy.Serialize(bytes, origin);
				Vector3Proxy.Serialize(bytes, direction);
				Int32Proxy.Serialize(bytes, itemId);
				ByteProxy.Serialize(bytes, playerNumber);
				Int32Proxy.Serialize(bytes, projectileID);

				SendEvent((byte)IGameRoomEventsType.EmitQuickItem, bytes);
			}
		}

		public void SendActivateQuickItem(int cmid, QuickItemLogic logic, int robotLifeTime, int scrapsLifeTime, bool isInstant) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				EnumProxy<QuickItemLogic>.Serialize(bytes, logic);
				Int32Proxy.Serialize(bytes, robotLifeTime);
				Int32Proxy.Serialize(bytes, scrapsLifeTime);
				BooleanProxy.Serialize(bytes, isInstant);

				SendEvent((byte)IGameRoomEventsType.ActivateQuickItem, bytes);
			}
		}

		public void SendChatMessage(int cmid, string name, string message, MemberAccessLevel accessLevel, byte context) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, cmid);
				StringProxy.Serialize(bytes, name);
				StringProxy.Serialize(bytes, message);
				EnumProxy<MemberAccessLevel>.Serialize(bytes, accessLevel);
				ByteProxy.Serialize(bytes, context);

				SendEvent((byte)IGameRoomEventsType.ChatMessage, bytes);
			}
		}
	}
}
