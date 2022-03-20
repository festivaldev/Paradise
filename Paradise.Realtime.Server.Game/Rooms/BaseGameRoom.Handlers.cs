using Paradise.Core.Models;
using Paradise.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public abstract partial class BaseGameRoom : BaseGameRoomOperationsHandler, IRoom<GamePeer>, IDisposable {
		protected override void OnJoinGame(GamePeer peer, TeamID team) {
			peer.Actor.Team = team;
			peer.Actor.Info.Health = 100;
			peer.Actor.Info.Ping = (ushort)(peer.RoundTripTime / 2);
			peer.Actor.Info.PlayerState = PlayerStates.Ready;
			peer.Actor.Info.SkinColor = Color.white;

			lock (_peers) {
				if (_players.FirstOrDefault(_ => _.Cmid == peer.Actor.Cmid) == null) {
					_players.Add(peer.Actor);
				}
			}

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
			OnPlayerRespawned(new PlayerRespawnedEventArgs { Player = peer });
		}

		protected override void OnDirectHitDamage(GamePeer peer, int target, byte bodyPart, byte bullets) {
			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != target) continue;

				// TODO: Actual damage calculation
				var damage = 10;

				var shortDamage = (short)damage;

				var victimPos = otherPeer.Actor.Movement.Position;
				var attackerPos = peer.Actor.Movement.Position;

				var direction = attackerPos - victimPos;
				var back = new Vector3(0, 0, -1);

				var angle = Vector3.Angle(direction, back);
				if (direction.x < 0)
					angle = 360 - angle;

				var byteAngle = Conversions.Angle2Byte(angle);

				otherPeer.Actor.Damage.AddDamage(byteAngle, shortDamage, bodyPart, 0, 0);
				otherPeer.Actor.Info.Health -= shortDamage;

				if (otherPeer.Actor.Info.Health <= 0) {
					otherPeer.Actor.Info.PlayerState |= PlayerStates.Dead;
					otherPeer.Actor.Info.Deaths++;
					peer.Actor.Info.Kills++;

					//peer.IncrementKills(weapon.ItemClass, part);

					//otherPeer.State.SetState(GamePeerState.Id.Killed);
					OnPlayerKilled(new PlayerKilledEventArgs {
						AttackerCmid = peer.Actor.Cmid,
						VictimCmid = otherPeer.Actor.Cmid,
						ItemClass = UberstrikeItemClass.WeaponHandgun,
						Damage = (ushort)shortDamage,
						Part = (BodyPart)bodyPart,
						Direction = -NormalizeVector(direction)
					});
				}
			}
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
			var state = peer.Actor.Info.PlayerState;
			if (on) {
				state |= PlayerStates.Shooting;
			} else {
				state &= ~PlayerStates.Shooting;
			}

			peer.Actor.Info.PlayerState = state;
		}

		protected override void OnIsReadyForNextMatch(GamePeer peer, bool on) {
			throw new NotImplementedException();
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

		protected override void OnIsInSniperMode(GamePeer peer, bool on) {
			var state = peer.Actor.Info.PlayerState;
			if (on) {
				state |= PlayerStates.Sniping;
			} else {
				state &= ~PlayerStates.Sniping;
			}

			peer.Actor.Info.PlayerState = state;
		}

		protected override void OnSingleBulletFire(GamePeer peer) {
			foreach (var otherPeer in Peers) {
				if (peer.Actor.Cmid != otherPeer.Actor.Cmid) {
					otherPeer.Events.Game.SendSingleBulletFire(peer.Actor.Cmid);
				}
			}
		}

		protected override void OnSwitchWeapon(GamePeer peer, byte weaponSlot) {
			peer.Actor.Info.CurrentWeaponSlot = weaponSlot;
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
			var actor = peer.Actor;

			var cmid = actor.Cmid;
			var playerName = actor.Info.PlayerName;
			var accessLevel = actor.Info.AccessLevel;

			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != cmid) {
					otherPeer.Events.Game.SendChatMessage(
						cmid,
						playerName,
						message,
						accessLevel,
						context
					);
				}
			}
		}

		private Vector3 NormalizeVector(Vector3 vector) {
			float magnitude = vector.magnitude;
			if (magnitude > 1E-05f) {
				return vector / magnitude;
			}
			return new Vector3(0, 0, 0);
		}
	}
}
