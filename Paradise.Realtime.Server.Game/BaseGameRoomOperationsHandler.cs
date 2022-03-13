using log4net;
using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public abstract class BaseGameRoomOperationsHandler : BaseOperationHandler<GamePeer> {
		private static readonly ILog Log = LogManager.GetLogger(typeof(BaseGameRoomOperationsHandler).Name);

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
			var operation = (IGameRoomOperationsType)(opCode);
			switch (operation) {
				case IGameRoomOperationsType.JoinGame:
					JoinGame(peer, bytes);
					break;
				case IGameRoomOperationsType.JoinAsSpectator:
					JoinAsSpectator(peer, bytes);
					break;
				case IGameRoomOperationsType.PowerUpRespawnTimes:
					PowerUpRespawnTimes(peer, bytes);
					break;
				case IGameRoomOperationsType.PowerUpPicked:
					PowerUpPicked(peer, bytes);
					break;
				case IGameRoomOperationsType.IncreaseHealthAndArmor:
					IncreaseHealthAndArmor(peer, bytes);
					break;
				case IGameRoomOperationsType.OpenDoor:
					OpenDoor(peer, bytes);
					break;
				case IGameRoomOperationsType.SpawnPositions:
					SpawnPositions(peer, bytes);
					break;
				case IGameRoomOperationsType.RespawnRequest:
					RespawnRequest(peer, bytes);
					break;
				case IGameRoomOperationsType.DirectHitDamage:
					DirectHitDamage(peer, bytes);
					break;
				case IGameRoomOperationsType.ExplosionDamage:
					ExplosionDamage(peer, bytes);
					break;
				case IGameRoomOperationsType.DirectDamage:
					DirectDamage(peer, bytes);
					break;
				case IGameRoomOperationsType.DirectDeath:
					DirectDeath(peer, bytes);
					break;
				case IGameRoomOperationsType.Jump:
					Jump(peer, bytes);
					break;
				case IGameRoomOperationsType.UpdatePositionAndRotation:
					UpdatePositionAndRotation(peer, bytes);
					break;
				case IGameRoomOperationsType.KickPlayer:
					KickPlayer(peer, bytes);
					break;
				case IGameRoomOperationsType.IsFiring:
					IsFiring(peer, bytes);
					break;
				case IGameRoomOperationsType.IsReadyForNextMatch:
					IsReadyForNextMatch(peer, bytes);
					break;
				case IGameRoomOperationsType.IsPaused:
					IsPaused(peer, bytes);
					break;
				case IGameRoomOperationsType.IsInSniperMode:
					IsInSniperMode(peer, bytes);
					break;
				case IGameRoomOperationsType.SingleBulletFire:
					SingleBulletFire(peer, bytes);
					break;
				case IGameRoomOperationsType.SwitchWeapon:
					SwitchWeapon(peer, bytes);
					break;
				case IGameRoomOperationsType.SwitchTeam:
					SwitchTeam(peer, bytes);
					break;
				case IGameRoomOperationsType.ChangeGear:
					ChangeGear(peer, bytes);
					break;
				case IGameRoomOperationsType.EmitProjectile:
					EmitProjectile(peer, bytes);
					break;
				case IGameRoomOperationsType.EmitQuickItem:
					EmitQuickItem(peer, bytes);
					break;
				case IGameRoomOperationsType.RemoveProjectile:
					RemoveProjectile(peer, bytes);
					break;
				case IGameRoomOperationsType.HitFeedback:
					HitFeedback(peer, bytes);
					break;
				case IGameRoomOperationsType.ActivateQuickItem:
					ActivateQuickItem(peer, bytes);
					break;
				case IGameRoomOperationsType.ChatMessage:
					ChatMessage(peer, bytes);
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private void JoinGame(GamePeer peer, MemoryStream bytes) {
			var team = EnumProxy<TeamID>.Deserialize(bytes);

			DebugOperation(peer, team);

			OnJoinGame(peer, team);
		}

		private void JoinAsSpectator(GamePeer peer, MemoryStream bytes) {
			DebugOperation(peer);

			OnJoinAsSpectator(peer);
		}

		private void PowerUpRespawnTimes(GamePeer peer, MemoryStream bytes) {
			var respawnTimes = ListProxy<ushort>.Deserialize(bytes, UInt16Proxy.Deserialize);

			DebugOperation(peer, respawnTimes);

			OnPowerUpRespawnTimes(peer, respawnTimes);
		}

		private void PowerUpPicked(GamePeer peer, MemoryStream bytes) {
			var powerupId = Int32Proxy.Deserialize(bytes);
			var type = ByteProxy.Deserialize(bytes);
			var value = ByteProxy.Deserialize(bytes);

			DebugOperation(peer, powerupId, type, value);

			OnPowerUpPicked(peer, powerupId, type, value);
		}

		private void IncreaseHealthAndArmor(GamePeer peer, MemoryStream bytes) {
			var health = ByteProxy.Deserialize(bytes);
			var armor = ByteProxy.Deserialize(bytes);

			DebugOperation(peer, health, armor);

			OnIncreaseHealthAndArmor(peer, health, armor);
		}

		private void OpenDoor(GamePeer peer, MemoryStream bytes) {
			var doorId = Int32Proxy.Deserialize(bytes);

			DebugOperation(peer, doorId);

			OnOpenDoor(peer, doorId);
		}

		private void SpawnPositions(GamePeer peer, MemoryStream bytes) {
			var team = EnumProxy<TeamID>.Deserialize(bytes);
			var positions = ListProxy<Vector3>.Deserialize(bytes, Vector3Proxy.Deserialize);
			var rotations = ListProxy<byte>.Deserialize(bytes, ByteProxy.Deserialize);

			DebugOperation(peer, team, positions, rotations);

			OnSpawnPositions(peer, team, positions, rotations);
		}

		private void RespawnRequest(GamePeer peer, MemoryStream bytes) {
			DebugOperation(peer);

			OnRespawnRequest(peer);
		}

		private void DirectHitDamage(GamePeer peer, MemoryStream bytes) {
			var target = Int32Proxy.Deserialize(bytes);
			var bodyPart = ByteProxy.Deserialize(bytes);
			var bullets = ByteProxy.Deserialize(bytes);

			DebugOperation(peer, target, bodyPart, bullets);

			OnDirectHitDamage(peer, target, bodyPart, bullets);
		}

		private void ExplosionDamage(GamePeer peer, MemoryStream bytes) {
			var target = Int32Proxy.Deserialize(bytes);
			var slot = ByteProxy.Deserialize(bytes);
			var distance = ByteProxy.Deserialize(bytes);
			var force = Vector3Proxy.Deserialize(bytes);

			DebugOperation(peer, target, slot, distance, force);

			OnExplosionDamage(peer, target, slot, distance, force);
		}

		private void DirectDamage(GamePeer peer, MemoryStream bytes) {
			var damage = UInt16Proxy.Deserialize(bytes);

			DebugOperation(peer, damage);

			OnDirectDamage(peer, damage);
		}

		private void DirectDeath(GamePeer peer, MemoryStream bytes) {
			DebugOperation(peer);

			OnDirectDeath(peer);
		}

		private void Jump(GamePeer peer, MemoryStream bytes) {
			var position = Vector3Proxy.Deserialize(bytes);

			DebugOperation(peer, position);

			OnJump(peer, position);
		}

		private void UpdatePositionAndRotation(GamePeer peer, MemoryStream bytes) {
			var position = ShortVector3Proxy.Deserialize(bytes);
			var velocity = ShortVector3Proxy.Deserialize(bytes);
			var hrot = ByteProxy.Deserialize(bytes);
			var vrot = ByteProxy.Deserialize(bytes);
			var moveState = ByteProxy.Deserialize(bytes);

			//DebugOperation(peer, position, velocity, hrot, vrot, moveState);

			OnUpdatePositionAndRotation(peer, position, velocity, hrot, vrot, moveState);
		}

		private void KickPlayer(GamePeer peer, MemoryStream bytes) {
			var cmid = Int32Proxy.Deserialize(bytes);

			DebugOperation(peer, cmid);

			OnKickPlayer(peer, cmid);
		}

		private void IsFiring(GamePeer peer, MemoryStream bytes) {
			var on = BooleanProxy.Deserialize(bytes);

			DebugOperation(peer, on);

			OnIsFiring(peer, on);
		}

		private void IsReadyForNextMatch(GamePeer peer, MemoryStream bytes) {
			var on = BooleanProxy.Deserialize(bytes);

			DebugOperation(peer, on);

			OnIsReadyForNextMatch(peer, on);
		}

		private void IsPaused(GamePeer peer, MemoryStream bytes) {
			var on = BooleanProxy.Deserialize(bytes);

			DebugOperation(peer, on);

			OnIsPaused(peer, on);
		}

		private void IsInSniperMode(GamePeer peer, MemoryStream bytes) {
			var on = BooleanProxy.Deserialize(bytes);

			DebugOperation(peer, on);

			OnIsInSniperMode(peer, on);
		}

		private void SingleBulletFire(GamePeer peer, MemoryStream bytes) {
			DebugOperation(peer);

			OnSingleBulletFire(peer);
		}

		private void SwitchWeapon(GamePeer peer, MemoryStream bytes) {
			var weaponSlot = ByteProxy.Deserialize(bytes);

			DebugOperation(peer, weaponSlot);

			OnSwitchWeapon(peer, weaponSlot);
		}

		private void SwitchTeam(GamePeer peer, MemoryStream bytes) {
			DebugOperation(peer);

			OnSwitchTeam(peer);
		}

		private void ChangeGear(GamePeer peer, MemoryStream bytes) {
			var head = Int32Proxy.Deserialize(bytes);
			var face = Int32Proxy.Deserialize(bytes);
			var upperBody = Int32Proxy.Deserialize(bytes);
			var lowerBody = Int32Proxy.Deserialize(bytes);
			var gloves = Int32Proxy.Deserialize(bytes);
			var boots = Int32Proxy.Deserialize(bytes);
			var holo = Int32Proxy.Deserialize(bytes);

			DebugOperation(peer, head, face, upperBody, lowerBody, gloves, boots, holo);

			OnChangeGear(peer, head, face, upperBody, lowerBody, gloves, boots, holo);
		}

		private void EmitProjectile(GamePeer peer, MemoryStream bytes) {
			var origin = Vector3Proxy.Deserialize(bytes);
			var direction = Vector3Proxy.Deserialize(bytes);
			var slot = ByteProxy.Deserialize(bytes);
			var projectileID = Int32Proxy.Deserialize(bytes);
			var explode = BooleanProxy.Deserialize(bytes);

			DebugOperation(peer, origin, direction, slot, projectileID, explode);

			OnEmitProjectile(peer, origin, direction, slot, projectileID, explode);
		}

		private void EmitQuickItem(GamePeer peer, MemoryStream bytes) {
			var origin = Vector3Proxy.Deserialize(bytes);
			var direction = Vector3Proxy.Deserialize(bytes);
			var itemId = Int32Proxy.Deserialize(bytes);
			var playerNumber = ByteProxy.Deserialize(bytes);
			var projectileID = Int32Proxy.Deserialize(bytes);

			DebugOperation(peer, origin, direction, itemId, playerNumber, projectileID);

			OnEmitQuickItem(peer, origin, direction, itemId, playerNumber, projectileID);
		}

		private void RemoveProjectile(GamePeer peer, MemoryStream bytes) {
			var projectileId = Int32Proxy.Deserialize(bytes);
			var explode = BooleanProxy.Deserialize(bytes);

			DebugOperation(peer, projectileId, explode);

			OnRemoveProjectile(peer, projectileId, explode);
		}

		private void HitFeedback(GamePeer peer, MemoryStream bytes) {
			var targetCmid = Int32Proxy.Deserialize(bytes);
			var force = Vector3Proxy.Deserialize(bytes);

			DebugOperation(peer, targetCmid, force);

			OnHitFeedback(peer, targetCmid, force);
		}

		private void ActivateQuickItem(GamePeer peer, MemoryStream bytes) {
			var logic = EnumProxy<QuickItemLogic>.Deserialize(bytes);
			var robotLifeTime = Int32Proxy.Deserialize(bytes);
			var scrapsLifeTime = Int32Proxy.Deserialize(bytes);
			var isInstant = BooleanProxy.Deserialize(bytes);

			DebugOperation(peer, logic, robotLifeTime, scrapsLifeTime, isInstant);

			OnActivateQuickItem(peer, logic, robotLifeTime, scrapsLifeTime, isInstant);
		}

		private void ChatMessage(GamePeer peer, MemoryStream bytes) {
			var message = StringProxy.Deserialize(bytes);
			var context = ByteProxy.Deserialize(bytes);

			DebugOperation(peer, message, context);

			OnChatMessage(peer, message, context);
		}

		private void DebugOperation(params object[] data) {
#if DEBUG
			Log.Info($"[{DateTime.UtcNow.ToString("o")}] {GetType().Name}:{new StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
#endif
		}
	}
}