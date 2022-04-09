using Paradise.Core.Models;
using Paradise.Core.Models.Views;
using Paradise.Core.Types;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public abstract partial class BaseGameRoom : BaseGameRoomOperationsHandler {
		private const float ARMOR_ABSORPTION = 0.66f;

		public override void OnDisconnect(GamePeer peer, DisconnectReason reason, string reasonDetail) {
			var room = peer.Room;

			if (room != null) {
				room.Leave(peer);

				if (room.Peers.Count <= 0) {
					GameApplication.Instance.RoomManager.RemoveRoom(room.RoomId);
				}

				peer.Dispose();
			}
		}

		protected override void OnJoinGame(GamePeer peer, TeamID team) {
			peer.Actor.Team = team;
			peer.Actor.Info.Health = 100;
			peer.Actor.Info.ArmorPoints = peer.Actor.Info.ArmorPointCapacity;
			peer.Actor.Info.Ping = (ushort)(peer.RoundTripTime / 2);
			peer.Actor.Info.PlayerState = PlayerStates.None;
			peer.Actor.Info.SkinColor = Color.white;

			peer.Actor.Info.Kills = 0;
			peer.Actor.Info.Deaths = 0;

			lock (_peers) {
				if (_players.FirstOrDefault(_ => _.Actor.Cmid == peer.Actor.Cmid) == null) {
					_players.Add(peer);
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
			if (!PowerUpManager.IsLoaded) {
				PowerUpManager.Load(respawnTimes);
			}
		}

		protected override void OnPowerUpPicked(GamePeer peer, int powerupId, byte type, byte value) {
			PowerUpManager.PickUp(peer, powerupId, (PickupItemType)type, value);
		}

		protected override void OnIncreaseHealthAndArmor(GamePeer peer, byte health, byte armor) {
			throw new NotImplementedException();
		}

		protected override void OnOpenDoor(GamePeer peer, int doorId) {
			throw new NotImplementedException();
		}

		protected override void OnSpawnPositions(GamePeer peer, TeamID team, List<Vector3> positions, List<byte> rotations) {
			if (!SpawnPointManager.IsLoaded(team)) {
				SpawnPointManager.Load(team, positions, rotations);
			}
		}

		protected override void OnRespawnRequest(GamePeer peer) {
			OnPlayerRespawned(new PlayerRespawnedEventArgs { Player = peer });
		}

		protected override void OnDirectHitDamage(GamePeer peer, int target, byte bodyPart, byte bullets) {
			foreach (var player in Players) {
				if (player.Actor.Cmid != target) continue;

				var weapon = default(UberStrikeItemWeaponView);

				switch (peer.Actor.Info.CurrentWeaponSlot) {
					case 0:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.MeleeWeapon, out weapon);
						break;
					case 1:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon1, out weapon);
						break;
					case 2:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon2, out weapon);
						break;
					case 3:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon3, out weapon);
						break;
					default: break;
				}

				if (weapon != null) {
					var damage = (weapon.DamagePerProjectile * bullets);

					var part = (BodyPart)bodyPart;
					var bonus = weapon.CriticalStrikeBonus;
					if (bonus > 0) {
						if (part == BodyPart.Head || part == BodyPart.Nuts) {
							damage = (int)Math.Round(damage + (damage * (bonus / 100f)));
						}
					}

					var shortDamage = (short)damage;

					var victimPos = player.Actor.Movement.Position;
					var attackerPos = peer.Actor.Movement.Position;

					var direction = attackerPos - victimPos;
					var back = new Vector3(0, 0, -1);

					var angle = Vector3.Angle(direction, back);
					if (direction.x < 0)
						angle = 360 - angle;

					var byteAngle = Conversions.Angle2Byte(angle);

					if (player.Actor.Info.ArmorPoints > 0) {
						int originalArmor = player.Actor.Info.ArmorPoints;
						player.Actor.Info.ArmorPoints = (byte)Math.Max(0, player.Actor.Info.ArmorPoints - shortDamage);

						double diff = (originalArmor - player.Actor.Info.ArmorPoints) * ARMOR_ABSORPTION;
						shortDamage -= (short)diff;
					}

					player.Actor.Damage.AddDamage(byteAngle, shortDamage, bodyPart, 0, 0);
					player.Actor.Info.Health -= shortDamage;

					peer.Actor.IncreaseWeaponShotsHit(weapon.ItemClass);
					peer.Actor.IncreaseWeaponDamageDone(weapon.ItemClass, shortDamage);

					player.Actor.IncreaseDamageReceived(shortDamage);

					if (player.Actor.Info.Health <= 0) {
						player.Actor.Info.PlayerState = PlayerStates.Dead;

						if (State.CurrentStateId == GameStateId.MatchRunning) {
							player.Actor.Info.Deaths++;
							peer.Actor.Info.Kills++;

							peer.Actor.IncreaseWeaponKills(weapon.ItemClass, (BodyPart)bodyPart);
						}

						OnPlayerKilled(new PlayerKilledEventArgs {
							AttackerCmid = peer.Actor.Cmid,
							VictimCmid = player.Actor.Cmid,
							ItemClass = weapon.ItemClass,
							Damage = (ushort)shortDamage,
							Part = (BodyPart)bodyPart,
							Direction = -NormalizeVector(direction)
						});
					}
				}
			}
		}

		protected override void OnExplosionDamage(GamePeer peer, int target, byte slot, byte distance, Vector3 force) {
			foreach (var player in Players) {
				if (player.Actor.Cmid != target) continue;

				var weapon = default(UberStrikeItemWeaponView);

				switch (peer.Actor.Info.CurrentWeaponSlot) {
					case 0:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.MeleeWeapon, out weapon);
						break;
					case 1:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon1, out weapon);
						break;
					case 2:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon2, out weapon);
						break;
					case 3:
						ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon3, out weapon);
						break;
					default: break;
				}

				if (weapon != null) {
					float damage = weapon.DamagePerProjectile;
					float radius = weapon.SplashRadius / 100f;
					float damageExplosion = damage * (radius - distance) / radius;

					var shortDamage = (short)damageExplosion;

					var victimPos = player.Actor.Movement.Position;
					var attackerPos = peer.Actor.Movement.Position;

					var direction = attackerPos - victimPos;
					var back = new Vector3(0, 0, -1);

					var angle = Vector3.Angle(direction, back);
					if (direction.x < 0)
						angle = 360 - angle;

					var byteAngle = Conversions.Angle2Byte(angle);

					if (player.Actor.Cmid != peer.Actor.Cmid) {
						if (player.Actor.Info.ArmorPoints > 0) {
							int originalArmor = player.Actor.Info.ArmorPoints;
							player.Actor.Info.ArmorPoints = (byte)Math.Max(0, player.Actor.Info.ArmorPoints - shortDamage);

							double diff = (originalArmor - player.Actor.Info.ArmorPoints) * ARMOR_ABSORPTION;
							shortDamage -= (short)diff;
						}

						player.Actor.Damage.AddDamage(byteAngle, shortDamage, (byte)BodyPart.Body, 0, 0);
					} else {
						shortDamage /= 2;

						if (player.Actor.Info.ArmorPoints > 0) {
							int originalArmor = player.Actor.Info.ArmorPoints;
							player.Actor.Info.ArmorPoints = (byte)Math.Max(0, player.Actor.Info.ArmorPoints - shortDamage);
							double diff = (originalArmor - player.Actor.Info.ArmorPoints) * ARMOR_ABSORPTION;
							shortDamage -= (short)diff;
						}
					}

					player.Actor.Info.Health -= shortDamage;

					peer.Actor.IncreaseWeaponShotsHit(weapon.ItemClass);
					peer.Actor.IncreaseWeaponDamageDone(weapon.ItemClass, shortDamage);

					if (player.Actor.Info.Health <= 0) {
						player.Actor.Info.PlayerState = PlayerStates.Dead;

						if (State.CurrentStateId == GameStateId.MatchRunning) {
							player.Actor.Info.Deaths++;
							peer.Actor.Info.Kills++;

							peer.Actor.IncreaseWeaponKills(weapon.ItemClass, BodyPart.Body);
						}

						OnPlayerKilled(new PlayerKilledEventArgs {
							AttackerCmid = peer.Actor.Cmid,
							VictimCmid = player.Actor.Cmid,
							ItemClass = weapon.ItemClass,
							Damage = (ushort)shortDamage,
							Part = BodyPart.Body,
							Direction = -(Vector3)direction
						});
					} else if (player.Actor.Cmid != peer.Actor.Cmid) {
						player.GameEvents.SendPlayerHit(force * weapon.DamageKnockback);
					}
				}
			}
		}

		protected override void OnDirectDamage(GamePeer peer, ushort damage) {
			throw new NotImplementedException();
		}

		protected override void OnDirectDeath(GamePeer peer) {
			peer.Actor.Info.PlayerState = PlayerStates.Dead;
			//peer.Actor.Info.Deaths++;

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
					otherPeer.GameEvents.SendPlayerJumped(peer.Actor.Cmid, peer.Actor.Movement.Position);
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
				peer.ShootingStartTime = Environment.TickCount;
				peer.ShootingWeapon = peer.Actor.Info.CurrentWeaponID;

				state |= PlayerStates.Shooting;
			} else {
				peer.ShootingEndTime = Environment.TickCount;

				var weapon = default(UberStrikeItemWeaponView);
				if (ShopManager.WeaponItems.TryGetValue(peer.ShootingWeapon, out weapon)) {
					TimeSpan shootTime = TimeSpan.FromMilliseconds(peer.ShootingEndTime - peer.ShootingStartTime);

					// TODO: Consider click spam?
					var shots = (int)Math.Ceiling(shootTime.TotalMilliseconds / weapon.RateOfFire);

					if (State.CurrentStateId == GameStateId.MatchRunning) {
						peer.Actor.IncreaseWeaponShotsFired(weapon.ItemClass, shots);
					}
				}

				peer.ShootingStartTime = 0;
				peer.ShootingEndTime = 0;
				peer.ShootingWeapon = 0;

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
			var weapon = default(UberStrikeItemWeaponView);

			switch (peer.Actor.Info.CurrentWeaponSlot) {
				case 0:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.MeleeWeapon, out weapon);
					break;
				case 1:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon1, out weapon);
					break;
				case 2:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon2, out weapon);
					break;
				case 3:
					ShopManager.WeaponItems.TryGetValue(peer.Loadout.Weapon3, out weapon);
					break;
				default: break;
			}

			if (weapon != null) {
				peer.Actor.IncreaseWeaponShotsFired(weapon.ItemClass, 1);
			}

			foreach (var otherPeer in Peers) {
				if (peer.Actor.Cmid != otherPeer.Actor.Cmid) {
					otherPeer.GameEvents.SendSingleBulletFire(peer.Actor.Cmid);
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
			var shooterCmid = peer.Actor.Cmid;

			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != shooterCmid) {
					otherPeer.GameEvents.SendEmitProjectile(shooterCmid, origin, direction, slot, projectileID, explode);
				}
			}
		}

		protected override void OnEmitQuickItem(GamePeer peer, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
			var emitterCmid = peer.Actor.Cmid;

			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != emitterCmid) {
					peer.GameEvents.SendEmitQuickItem(origin, direction, itemId, playerNumber, projectileID);
				}
			}
		}

		protected override void OnRemoveProjectile(GamePeer peer, int projectileID, bool explode) {
			foreach (var otherPeer in Peers) {
				otherPeer.GameEvents.SendRemoveProjectile(projectileID, explode);
			}
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

			Log.Info($"{playerName}: {message}");

			foreach (var otherPeer in Peers) {
				if (otherPeer.Actor.Cmid != cmid) {
					otherPeer.GameEvents.SendChatMessage(
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
