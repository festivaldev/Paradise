using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public abstract partial class BaseGameRoom : BaseGameRoomOperationsHandler, IRoom<GamePeer>, IDisposable {
		protected override void OnJoinGame(GamePeer peer, TeamID team) {
			OnPlayerJoined(new PlayerJoinedEventArgs {
				Player = peer,
				Team = team
			});
		}

		protected override void OnJoinAsSpectator(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnPowerUpRespawnTimes(GamePeer peer, List<ushort> respawnTimes) {
			throw new NotImplementedException();
		}

		protected override void OnPowerUpPicked(GamePeer peer, int powerupId, byte type, byte value) {
			throw new NotImplementedException();
		}

		protected override void OnIncreaseHealthAndArmor(GamePeer peer, byte health, byte armor) {
			throw new NotImplementedException();
		}

		protected override void OnOpenDoor(GamePeer peer, int doorId) {
			throw new NotImplementedException();
		}

		protected override void OnSpawnPositions(GamePeer peer, TeamID team, List<Vector3> positions, List<byte> rotations) {
			throw new NotImplementedException();
		}

		protected override void OnRespawnRequest(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnDirectHitDamage(GamePeer peer, int target, byte bodyPart, byte bullets) {
			throw new NotImplementedException();
		}

		protected override void OnExplosionDamage(GamePeer peer, int target, byte slot, byte distance, Vector3 force) {
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

		protected override void OnJump(GamePeer peer, Vector3 position) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != peer.Actor.Cmid) {
					otherPeer.Events.Game.SendPlayerJumped(peer.Actor.Cmid, peer.Actor.Movement.Position);
				}
			}
		}

		protected override void OnUpdatePositionAndRotation(GamePeer peer, ShortVector3 position, ShortVector3 velocity, byte hrot, byte vrot, byte moveState) {
			peer.Actor.Movement.Position = position;
			peer.Actor.Movement.Velocity = velocity;
			peer.Actor.Movement.HorizontalRotation = hrot;
			peer.Actor.Movement.VerticalRotation = vrot;
			peer.Actor.Movement.MovementState = moveState;
		}

		protected override void OnKickPlayer(GamePeer peer, int cmid) {
			throw new NotImplementedException();
		}

		protected override void OnIsFiring(GamePeer peer, bool on) {
			throw new NotImplementedException();
		}

		protected override void OnIsReadyForNextMatch(GamePeer peer, bool on) {
			throw new NotImplementedException();
		}

		protected override void OnIsPaused(GamePeer peer, bool on) {
			throw new NotImplementedException();
		}

		protected override void OnIsInSniperMode(GamePeer peer, bool on) {
			throw new NotImplementedException();
		}

		protected override void OnSingleBulletFire(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnSwitchWeapon(GamePeer peer, byte weaponSlot) {
			peer.Actor.Info.CurrentWeaponSlot = weaponSlot;
			Log.Info($"changed weapon slot, delta count: {peer.Actor.Info.Delta.Changes.Count}");
		}

		protected override void OnSwitchTeam(GamePeer peer) {
			throw new NotImplementedException();
		}

		protected override void OnChangeGear(GamePeer peer, int head, int face, int upperBody, int lowerBody, int gloves, int boots, int holo) {
			throw new NotImplementedException();
		}

		protected override void OnEmitProjectile(GamePeer peer, Vector3 origin, Vector3 direction, byte slot, int projectileID, bool explode) {
			throw new NotImplementedException();
		}

		protected override void OnEmitQuickItem(GamePeer peer, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
			throw new NotImplementedException();
		}

		protected override void OnRemoveProjectile(GamePeer peer, int projectileID, bool explode) {
			throw new NotImplementedException();
		}

		protected override void OnHitFeedback(GamePeer peer, int targetCmid, Vector3 force) {
			throw new NotImplementedException();
		}

		protected override void OnActivateQuickItem(GamePeer peer, QuickItemLogic logic, int robotLifeTime, int scrapsLifeTime, bool isInstant) {
			throw new NotImplementedException();
		}

		protected override void OnChatMessage(GamePeer peer, string message, byte context) {
			throw new NotImplementedException();
		}
	}
}
