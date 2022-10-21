using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public abstract class BaseGameRoomOperationHandler : BaseOperationHandler<GamePeer> {
		private static readonly ILog Log = LogManager.GetLogger("GameLog");

		protected object _lock { get; } = new object();

		public override int Id => 0;

		protected abstract void OnJoinGame(GamePeer peer, TeamID team);
		protected abstract void OnJoinAsSpectator(GamePeer peer);
		protected abstract void OnPowerUpRespawnTimes(GamePeer peer, List<ushort> respawnTimes);
		protected abstract void OnPowerUpPicked(GamePeer peer, int powerupId, byte type, byte value);
		protected abstract void OnIncreaseHealthAndArmor(GamePeer peer, byte health, byte armor);
		protected abstract void OnOpenDoor(GamePeer peer, int doorId);
		protected abstract void OnSpawnPositions(GamePeer peer, TeamID team, List<Vector3> positions, List<byte> rotations);
		protected abstract void OnRespawnRequest(GamePeer peer);
		protected abstract void OnDirectHitDamage(GamePeer peer, int target, byte bodyPart, byte bullets);
		protected abstract void OnExplosionDamage(GamePeer peer, int target, byte slot, byte distance, Vector3 force);
		protected abstract void OnDirectDamage(GamePeer peer, ushort damage);
		protected abstract void OnDirectDeath(GamePeer peer);
		protected abstract void OnJump(GamePeer peer, Vector3 position);
		protected abstract void OnUpdatePositionAndRotation(GamePeer peer, ShortVector3 position, ShortVector3 velocity, byte hrot, byte vrot, byte moveState);
		protected abstract void OnKickPlayer(GamePeer peer, int cmid);
		protected abstract void OnIsFiring(GamePeer peer, bool on);
		protected abstract void OnIsReadyForNextMatch(GamePeer peer, bool on);
		protected abstract void OnIsPaused(GamePeer peer, bool on);
		protected abstract void OnIsInSniperMode(GamePeer peer, bool on);
		protected abstract void OnSingleBulletFire(GamePeer peer);
		protected abstract void OnSwitchWeapon(GamePeer peer, byte weaponSlot);
		protected abstract void OnSwitchTeam(GamePeer peer);
		protected abstract void OnChangeGear(GamePeer peer, int head, int face, int upperBody, int lowerBody, int gloves, int boots, int holo);
		protected abstract void OnEmitProjectile(GamePeer peer, Vector3 origin, Vector3 direction, byte slot, int projectileID, bool explode);
		protected abstract void OnEmitQuickItem(GamePeer peer, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID);
		protected abstract void OnRemoveProjectile(GamePeer peer, int projectileID, bool explode);
		protected abstract void OnHitFeedback(GamePeer peer, int targetCmid, Vector3 force);
		protected abstract void OnActivateQuickItem(GamePeer peer, QuickItemLogic logic, int robotLifeTime, int scrapsLifeTime, bool isInstant);
		protected abstract void OnChatMessage(GamePeer peer, string message, byte context);

		public override void OnOperationRequest(GamePeer peer, byte opCode, MemoryStream bytes) {
			var operation = (IGameRoomOperationsType)opCode;

			switch (operation) {
				case IGameRoomOperationsType.UpdatePositionAndRotation:
					break;
				default:
					Log.Debug($"GameRoom.OnOperationRequest peer: {peer}, opCode: {operation}({opCode})");
					break;
			}

			switch (operation) {
				case IGameRoomOperationsType.JoinGame: {
					var team = EnumProxy<TeamID>.Deserialize(bytes);

					OnJoinGame(peer, team);
					break;
				}
				case IGameRoomOperationsType.JoinAsSpectator: {
					OnJoinAsSpectator(peer);
					break;
				}
				case IGameRoomOperationsType.PowerUpRespawnTimes: {
					var respawnTimes = ListProxy<ushort>.Deserialize(bytes, UInt16Proxy.Deserialize);

					OnPowerUpRespawnTimes(peer, respawnTimes);
					break;
				}
				case IGameRoomOperationsType.PowerUpPicked: {
					var powerupId = Int32Proxy.Deserialize(bytes);
					var type = ByteProxy.Deserialize(bytes);
					var value = ByteProxy.Deserialize(bytes);

					OnPowerUpPicked(peer, powerupId, type, value);
					break;
				}
				case IGameRoomOperationsType.IncreaseHealthAndArmor: {
					var health = ByteProxy.Deserialize(bytes);
					var armor = ByteProxy.Deserialize(bytes);

					OnIncreaseHealthAndArmor(peer, health, armor);
					break;
				}
				case IGameRoomOperationsType.OpenDoor: {
					var doorId = Int32Proxy.Deserialize(bytes);

					OnOpenDoor(peer, doorId);
					break;
				}
				case IGameRoomOperationsType.SpawnPositions: {
					var team = EnumProxy<TeamID>.Deserialize(bytes);
					var positions = ListProxy<Vector3>.Deserialize(bytes, Vector3Proxy.Deserialize);
					var rotations = ListProxy<byte>.Deserialize(bytes, ByteProxy.Deserialize);

					OnSpawnPositions(peer, team, positions, rotations);
					break;
				}
				case IGameRoomOperationsType.RespawnRequest: {
					OnRespawnRequest(peer);
					break;
				}
				case IGameRoomOperationsType.DirectHitDamage: {
					var target = Int32Proxy.Deserialize(bytes);
					var bodyPart = ByteProxy.Deserialize(bytes);
					var bullets = ByteProxy.Deserialize(bytes);

					OnDirectHitDamage(peer, target, bodyPart, bullets);
					break;
				}
				case IGameRoomOperationsType.ExplosionDamage: {
					var target = Int32Proxy.Deserialize(bytes);
					var slot = ByteProxy.Deserialize(bytes);
					var distance = ByteProxy.Deserialize(bytes);
					var force = Vector3Proxy.Deserialize(bytes);

					OnExplosionDamage(peer, target, slot, distance, force);
					break;
				}
				case IGameRoomOperationsType.DirectDamage: {
					var damage = UInt16Proxy.Deserialize(bytes);

					OnDirectDamage(peer, damage);
					break;
				}
				case IGameRoomOperationsType.DirectDeath: {
					OnDirectDeath(peer);
					break;
				}
				case IGameRoomOperationsType.Jump: {
					var position = Vector3Proxy.Deserialize(bytes);

					OnJump(peer, position);
					break;
				}
				case IGameRoomOperationsType.UpdatePositionAndRotation: {
					var position = ShortVector3Proxy.Deserialize(bytes);
					var velocity = ShortVector3Proxy.Deserialize(bytes);
					var hrot = ByteProxy.Deserialize(bytes);
					var vrot = ByteProxy.Deserialize(bytes);
					var moveState = ByteProxy.Deserialize(bytes);

					OnUpdatePositionAndRotation(peer, position, velocity, hrot, vrot, moveState);
					break;
				}
				case IGameRoomOperationsType.KickPlayer: {
					var cmid = Int32Proxy.Deserialize(bytes);

					OnKickPlayer(peer, cmid);
					break;
				}
				case IGameRoomOperationsType.IsFiring: {
					var on = BooleanProxy.Deserialize(bytes);

					OnIsFiring(peer, on);
					break;
				}
				case IGameRoomOperationsType.IsReadyForNextMatch: {
					var on = BooleanProxy.Deserialize(bytes);

					OnIsReadyForNextMatch(peer, on);
					break;
				}
				case IGameRoomOperationsType.IsPaused: {
					var on = BooleanProxy.Deserialize(bytes);

					OnIsPaused(peer, on);
					break;
				}
				case IGameRoomOperationsType.IsInSniperMode: {
					var on = BooleanProxy.Deserialize(bytes);

					OnIsInSniperMode(peer, on);
					break;
				}
				case IGameRoomOperationsType.SingleBulletFire: {
					OnSingleBulletFire(peer);
					break;
				}
				case IGameRoomOperationsType.SwitchWeapon: {
					var weaponSlot = ByteProxy.Deserialize(bytes);

					OnSwitchWeapon(peer, weaponSlot);
					break;
				}
				case IGameRoomOperationsType.SwitchTeam: {
					OnSwitchTeam(peer);
					break;
				}
				case IGameRoomOperationsType.ChangeGear: {
					var head = Int32Proxy.Deserialize(bytes);
					var face = Int32Proxy.Deserialize(bytes);
					var upperBody = Int32Proxy.Deserialize(bytes);
					var lowerBody = Int32Proxy.Deserialize(bytes);
					var gloves = Int32Proxy.Deserialize(bytes);
					var boots = Int32Proxy.Deserialize(bytes);
					var holo = Int32Proxy.Deserialize(bytes);

					OnChangeGear(peer, head, face, upperBody, lowerBody, gloves, boots, holo);
					break;
				}
				case IGameRoomOperationsType.EmitProjectile: {
					var origin = Vector3Proxy.Deserialize(bytes);
					var direction = Vector3Proxy.Deserialize(bytes);
					var slot = ByteProxy.Deserialize(bytes);
					var projectileID = Int32Proxy.Deserialize(bytes);
					var explode = BooleanProxy.Deserialize(bytes);

					OnEmitProjectile(peer, origin, direction, slot, projectileID, explode);
					break;
				}
				case IGameRoomOperationsType.EmitQuickItem: {
					var origin = Vector3Proxy.Deserialize(bytes);
					var direction = Vector3Proxy.Deserialize(bytes);
					var itemId = Int32Proxy.Deserialize(bytes);
					var playerNumber = ByteProxy.Deserialize(bytes);
					var projectileID = Int32Proxy.Deserialize(bytes);

					OnEmitQuickItem(peer, origin, direction, itemId, playerNumber, projectileID);
					break;
				}
				case IGameRoomOperationsType.RemoveProjectile: {
					var projectileId = Int32Proxy.Deserialize(bytes);
					var explode = BooleanProxy.Deserialize(bytes);

					OnRemoveProjectile(peer, projectileId, explode);
					break;
				}
				case IGameRoomOperationsType.HitFeedback: {
					var targetCmid = Int32Proxy.Deserialize(bytes);
					var force = Vector3Proxy.Deserialize(bytes);

					OnHitFeedback(peer, targetCmid, force);
					break;
				}
				case IGameRoomOperationsType.ActivateQuickItem: {
					var logic = EnumProxy<QuickItemLogic>.Deserialize(bytes);
					var robotLifeTime = Int32Proxy.Deserialize(bytes);
					var scrapsLifeTime = Int32Proxy.Deserialize(bytes);
					var isInstant = BooleanProxy.Deserialize(bytes);

					OnActivateQuickItem(peer, logic, robotLifeTime, scrapsLifeTime, isInstant);
					break;
				}
				case IGameRoomOperationsType.ChatMessage: {
					var message = StringProxy.Deserialize(bytes);
					var context = ByteProxy.Deserialize(bytes);

					OnChatMessage(peer, message, context);
					break;
				}
				default:
					throw new NotSupportedException();
			}
		}

		
	}
}
