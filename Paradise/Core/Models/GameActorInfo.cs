using Paradise.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Core.Models {
	[Synchronizable]
	[Serializable]
	public class GameActorInfo {
		public GameActorInfo() {
			this.Weapons = new List<int>
			{
				0,
				0,
				0,
				0
			};
			this.Gear = new List<int>
			{
				0,
				0,
				0,
				0,
				0,
				0,
				0
			};
			this.QuickItems = new List<int>
			{
				0,
				0,
				0
			};
			this.FunctionalItems = new List<int>
			{
				0,
				0,
				0
			};
		}

		public int Cmid { get; set; }

		public string PlayerName { get; set; }

		public MemberAccessLevel AccessLevel { get; set; }

		public ChannelType Channel { get; set; }

		public string ClanTag { get; set; }

		public byte Rank { get; set; }

		public byte PlayerId { get; set; }

		public PlayerStates PlayerState { get; set; }

		public short Health { get; set; }

		public TeamID TeamID { get; set; }

		public int Level { get; set; }

		public ushort Ping { get; set; }

		public byte CurrentWeaponSlot { get; set; }

		public FireMode CurrentFiringMode { get; set; }

		public byte ArmorPoints { get; set; }

		public byte ArmorPointCapacity { get; set; }

		public Color SkinColor { get; set; }

		public short Kills { get; set; }

		public short Deaths { get; set; }

		public List<int> Weapons { get; set; }

		public List<int> Gear { get; set; }

		public List<int> FunctionalItems { get; set; }

		public List<int> QuickItems { get; set; }

		public SurfaceType StepSound { get; set; }

		public bool IsFiring {
			get {
				return this.Is(PlayerStates.Shooting);
			}
		}

		public bool IsReadyForGame {
			get {
				return this.Is(PlayerStates.Ready);
			}
		}

		public bool IsOnline {
			get {
				return !this.Is(PlayerStates.Offline);
			}
		}

		public bool Is(PlayerStates state) {
			return (byte)(this.PlayerState & state) != 0;
		}

		public int CurrentWeaponID {
			get {
				return (this.Weapons == null || this.Weapons.Count <= (int)this.CurrentWeaponSlot) ? 0 : this.Weapons[(int)this.CurrentWeaponSlot];
			}
		}

		public bool IsAlive {
			get {
				return (byte)(this.PlayerState & PlayerStates.Dead) == 0;
			}
		}

		public bool IsSpectator {
			get {
				return (byte)(this.PlayerState & PlayerStates.Spectator) != 0;
			}
		}

		public float GetAbsorptionRate() {
			return 0.66f;
		}

		public void Damage(short damage, BodyPart part, out short healthDamage, out byte armorDamage) {
			if (this.ArmorPoints > 0) {
				int value = Mathf.CeilToInt(this.GetAbsorptionRate() * (float)damage);
				armorDamage = (byte)Mathf.Clamp(value, 0, (int)this.ArmorPoints);
				healthDamage = (short)(damage - armorDamage);
			} else {
				armorDamage = 0;
				healthDamage = damage;
			}
		}
	}
}
