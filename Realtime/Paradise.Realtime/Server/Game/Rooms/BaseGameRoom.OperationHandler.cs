using log4net;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using UberStrike.Core.Models;
using UberStrike.Core.Serialization;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public partial class BaseGameRoom {
		public class OperationHandler : BaseOperationHandler<GamePeer, IGameRoomOperationsType> {
			private static readonly new ILog Log = LogManager.GetLogger(nameof(BaseGameRoom.OperationHandler));
			private readonly BaseGameRoom GameRoom;

			public override int Id => (int)OperationHandlerId.GameRoom;
			public override Dictionary<IGameRoomOperationsType, int> RateLimiterIntervals => new Dictionary<IGameRoomOperationsType, int> {
				[IGameRoomOperationsType.SwitchWeapon] = 50,
				[IGameRoomOperationsType.ChatMessage] = 1000
			};

			public OperationHandler(BaseGameRoom gameRoom) : base() {
				GameRoom = gameRoom;
			}

			public override void OnOperationRequest(GamePeer peer, byte opCode, MemoryStream bytes) {
				switch ((IGameRoomOperationsType)opCode) {
					case IGameRoomOperationsType.UpdatePositionAndRotation:
						break;
					default:
						Log.Debug($"BaseGameRoom.OperationHandler::OnOperationRequest -> peer: {peer}, opCode: {(IGameRoomOperationsType)opCode}({opCode})");
						break;
				}

				switch ((IGameRoomOperationsType)opCode) {
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

			public override void OnDisconnect(GamePeer peer, DisconnectReason reason, string reasonDetail) {
				var room = peer.Room;

				if (room != null) {
					room.Leave(peer);

					if (room.Peers.Count <= 0) {
						GameServerApplication.Instance.RoomManager.RemoveRoom(room.RoomId);
					}

					peer.Dispose();
				}
			}

			#region Implementation of IGameRoomOperationsType
			private void JoinGame(GamePeer peer, MemoryStream bytes) {
				var team = EnumProxy<TeamID>.Deserialize(bytes);

				DebugOperation(peer, team);

				GameRoom.JoinGame(peer, team);
			}

			private void JoinAsSpectator(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				GameRoom.JoinAsSpectator(peer);
			}

			private void PowerUpRespawnTimes(GamePeer peer, MemoryStream bytes) {
				var respawnTimes = ListProxy<ushort>.Deserialize(bytes, UInt16Proxy.Deserialize);
				var positions = ListProxy<Vector3>.Deserialize(bytes, Vector3Proxy.Deserialize);

				DebugOperation(peer, respawnTimes);

				GameRoom.PowerUpRespawnTimes(peer, respawnTimes, positions);
			}

			private void PowerUpPicked(GamePeer peer, MemoryStream bytes) {
				var powerupId = Int32Proxy.Deserialize(bytes);
				var type = ByteProxy.Deserialize(bytes);
				var value = ByteProxy.Deserialize(bytes);

				DebugOperation(peer, powerupId, type, value);

				GameRoom.PowerUpPicked(peer, powerupId, type, value);
			}

			private void IncreaseHealthAndArmor(GamePeer peer, MemoryStream bytes) {
				var health = ByteProxy.Deserialize(bytes);
				var armor = ByteProxy.Deserialize(bytes);

				DebugOperation(peer, health, armor);

				GameRoom.IncreaseHealthAndArmor(peer, health, armor);
			}

			private void OpenDoor(GamePeer peer, MemoryStream bytes) {
				var doorId = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, doorId);

				GameRoom.OpenDoor(peer, doorId);
			}

			private void SpawnPositions(GamePeer peer, MemoryStream bytes) {
				var team = EnumProxy<TeamID>.Deserialize(bytes);
				var positions = ListProxy<Vector3>.Deserialize(bytes, Vector3Proxy.Deserialize);
				var rotations = ListProxy<byte>.Deserialize(bytes, ByteProxy.Deserialize);

				DebugOperation(peer, team, positions, rotations);

				GameRoom.SpawnPositions(peer, team, positions, rotations);
			}

			private void RespawnRequest(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				GameRoom.RespawnRequest(peer);
			}

			private void DirectHitDamage(GamePeer peer, MemoryStream bytes) {
				var target = Int32Proxy.Deserialize(bytes);
				var bodyPart = ByteProxy.Deserialize(bytes);
				var bullets = ByteProxy.Deserialize(bytes);

				DebugOperation(peer, target, bodyPart, bullets);

				GameRoom.DirectHitDamage(peer, target, bodyPart, bullets);
			}

			private void ExplosionDamage(GamePeer peer, MemoryStream bytes) {
				var target = Int32Proxy.Deserialize(bytes);
				var slot = ByteProxy.Deserialize(bytes);
				var distance = ByteProxy.Deserialize(bytes);
				var force = Vector3Proxy.Deserialize(bytes);

				DebugOperation(peer, target, slot, distance, force);

				GameRoom.ExplosionDamage(peer, target, slot, distance, force);
			}

			private void DirectDamage(GamePeer peer, MemoryStream bytes) {
				var damage = UInt16Proxy.Deserialize(bytes);

				DebugOperation(peer, damage);

				GameRoom.DirectDamage(peer, damage);
			}

			private void DirectDeath(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				GameRoom.DirectDeath(peer);
			}

			private void Jump(GamePeer peer, MemoryStream bytes) {
				var position = Vector3Proxy.Deserialize(bytes);

				DebugOperation(peer, position);

				GameRoom.Jump(peer, position);
			}

			private void UpdatePositionAndRotation(GamePeer peer, MemoryStream bytes) {
				var position = ShortVector3Proxy.Deserialize(bytes);
				var velocity = ShortVector3Proxy.Deserialize(bytes);
				var hrot = ByteProxy.Deserialize(bytes);
				var vrot = ByteProxy.Deserialize(bytes);
				var moveState = ByteProxy.Deserialize(bytes);

				DebugOperation(peer, position, velocity, hrot, vrot, moveState);

				GameRoom.UpdatePositionAndRotation(peer, position, velocity, hrot, vrot, moveState);
			}

			private void KickPlayer(GamePeer peer, MemoryStream bytes) {
				var cmid = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, cmid);

				GameRoom.KickPlayer(peer, cmid);
			}

			private void IsFiring(GamePeer peer, MemoryStream bytes) {
				var on = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, on);

				GameRoom.IsFiring(peer, on);
			}

			private void IsReadyForNextMatch(GamePeer peer, MemoryStream bytes) {
				var on = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, on);

				GameRoom.IsReadyForNextMatch(peer, on);
			}

			private void IsPaused(GamePeer peer, MemoryStream bytes) {
				var on = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, on);

				GameRoom.IsPaused(peer, on);
			}

			private void IsInSniperMode(GamePeer peer, MemoryStream bytes) {
				var on = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, on);

				GameRoom.IsInSniperMode(peer, on);
			}

			private void SingleBulletFire(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				GameRoom.SingleBulletFire(peer);
			}

			private void SwitchWeapon(GamePeer peer, MemoryStream bytes) {
				var weaponSlot = ByteProxy.Deserialize(bytes);

				DebugOperation(peer, weaponSlot);

				GameRoom.SwitchWeapon(peer, weaponSlot);
			}

			private void SwitchTeam(GamePeer peer, MemoryStream bytes) {
				DebugOperation(peer);

				GameRoom.SwitchTeam(peer);
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

				GameRoom.ChangeGear(peer, head, face, upperBody, lowerBody, gloves, boots, holo);
			}

			private void EmitProjectile(GamePeer peer, MemoryStream bytes) {
				var origin = Vector3Proxy.Deserialize(bytes);
				var direction = Vector3Proxy.Deserialize(bytes);
				var slot = ByteProxy.Deserialize(bytes);
				var projectileID = Int32Proxy.Deserialize(bytes);
				var explode = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, origin, direction, slot, projectileID, explode);

				GameRoom.EmitProjectile(peer, origin, direction, slot, projectileID, explode);
			}

			private void EmitQuickItem(GamePeer peer, MemoryStream bytes) {
				var origin = Vector3Proxy.Deserialize(bytes);
				var direction = Vector3Proxy.Deserialize(bytes);
				var itemId = Int32Proxy.Deserialize(bytes);
				var playerNumber = ByteProxy.Deserialize(bytes);
				var projectileID = Int32Proxy.Deserialize(bytes);

				DebugOperation(peer, origin, direction, itemId, playerNumber, projectileID);

				GameRoom.EmitQuickItem(peer, origin, direction, itemId, playerNumber, projectileID);
			}

			private void RemoveProjectile(GamePeer peer, MemoryStream bytes) {
				var projectileId = Int32Proxy.Deserialize(bytes);
				var explode = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, projectileId, explode);

				GameRoom.RemoveProjectile(peer, projectileId, explode);
			}

			private void HitFeedback(GamePeer peer, MemoryStream bytes) {
				var targetCmid = Int32Proxy.Deserialize(bytes);
				var force = Vector3Proxy.Deserialize(bytes);

				DebugOperation(peer, targetCmid, force);

				GameRoom.HitFeedback(peer, targetCmid, force);
			}

			private void ActivateQuickItem(GamePeer peer, MemoryStream bytes) {
				var logic = EnumProxy<QuickItemLogic>.Deserialize(bytes);
				var robotLifeTime = Int32Proxy.Deserialize(bytes);
				var scrapsLifeTime = Int32Proxy.Deserialize(bytes);
				var isInstant = BooleanProxy.Deserialize(bytes);

				DebugOperation(peer, logic, robotLifeTime, scrapsLifeTime, isInstant);

				GameRoom.ActivateQuickItem(peer, logic, robotLifeTime, scrapsLifeTime, isInstant);
			}

			private void ChatMessage(GamePeer peer, MemoryStream bytes) {
				var message = StringProxy.Deserialize(bytes);
				var context = ByteProxy.Deserialize(bytes);

				DebugOperation(peer, message, context);

				GameRoom.ChatMessage(peer, message, context);
			}
			#endregion



			private void DebugOperation(params object[] data) {
#if DEBUG
				Log.Debug($"{GetType().Name}:{new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name} -> {string.Join(", ", data)}");
#endif
			}
		}
	}
}
