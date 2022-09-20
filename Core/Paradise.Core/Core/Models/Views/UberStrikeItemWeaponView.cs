using Paradise.Core.Types;
using System;
using UnityEngine;

namespace Paradise.Core.Models.Views {
	[Serializable]
	public class UberStrikeItemWeaponView : BaseUberStrikeItemView {
		public override UberstrikeItemType ItemType {
			get {
				return UberstrikeItemType.Weapon;
			}
		}

		public int DamageKnockback {
			get {
				return this._damageKnockback;
			}
			set {
				this._damageKnockback = value;
			}
		}

		public int DamagePerProjectile {
			get {
				return this._damagePerProjectile;
			}
			set {
				this._damagePerProjectile = value;
			}
		}

		public int AccuracySpread {
			get {
				return this._accuracySpread;
			}
			set {
				this._accuracySpread = value;
			}
		}

		public int RecoilKickback {
			get {
				return this._recoilKickback;
			}
			set {
				this._recoilKickback = value;
			}
		}

		public int StartAmmo {
			get {
				return this._startAmmo;
			}
			set {
				this._startAmmo = value;
			}
		}

		public int MaxAmmo {
			get {
				return this._maxAmmo;
			}
			set {
				this._maxAmmo = value;
			}
		}

		public int MissileTimeToDetonate {
			get {
				return this._missileTimeToDetonate;
			}
			set {
				this._missileTimeToDetonate = value;
			}
		}

		public int MissileForceImpulse {
			get {
				return this._missileForceImpulse;
			}
			set {
				this._missileForceImpulse = value;
			}
		}

		public int MissileBounciness {
			get {
				return this._missileBounciness;
			}
			set {
				this._missileBounciness = value;
			}
		}

		public int SplashRadius {
			get {
				return this._splashRadius;
			}
			set {
				this._splashRadius = value;
			}
		}

		public int ProjectilesPerShot {
			get {
				return this._projectilesPerShot;
			}
			set {
				this._projectilesPerShot = value;
			}
		}

		public int ProjectileSpeed {
			get {
				return this._projectileSpeed;
			}
			set {
				this._projectileSpeed = value;
			}
		}

		public int RateOfFire {
			get {
				return this._rateOfFire;
			}
			set {
				this._rateOfFire = value;
			}
		}

		public int RecoilMovement {
			get {
				return this._recoilMovement;
			}
			set {
				this._recoilMovement = value;
			}
		}

		public int CombatRange {
			get {
				return this._combatRange;
			}
			set {
				this._combatRange = value;
			}
		}

		public int Tier {
			get {
				return this._tier;
			}
			set {
				this._tier = value;
			}
		}

		public int SecondaryActionReticle {
			get {
				return this._secondaryActionReticle;
			}
			set {
				this._secondaryActionReticle = value;
			}
		}

		public int WeaponSecondaryAction {
			get {
				return this._weaponSecondaryAction;
			}
			set {
				this._weaponSecondaryAction = value;
			}
		}

		public int CriticalStrikeBonus {
			get {
				return this._criticalStrikeBonus;
			}
			set {
				this._criticalStrikeBonus = value;
			}
		}

		public float DamagePerSecond {
			get {
				return (float)((this.RateOfFire == 0) ? 0 : (this.DamagePerProjectile * this.ProjectilesPerShot / this.RateOfFire));
			}
		}

		public bool HasAutomaticFire {
			get {
				return this._hasAutoFire;
			}
			set {
				this._hasAutoFire = value;
			}
		}

		public int DefaultZoomMultiplier {
			get {
				return this._defaultZoomMultiplier;
			}
			set {
				this._defaultZoomMultiplier = value;
			}
		}

		public int MinZoomMultiplier {
			get {
				return this._minZoomMultiplier;
			}
			set {
				this._minZoomMultiplier = value;
			}
		}

		public int MaxZoomMultiplier {
			get {
				return this._maxZoomMultiplier;
			}
			set {
				this._maxZoomMultiplier = value;
			}
		}

		[SerializeField]
		private int _damageKnockback;

		[SerializeField]
		private int _damagePerProjectile;

		[SerializeField]
		private int _accuracySpread;

		[SerializeField]
		private int _recoilKickback;

		[SerializeField]
		private int _startAmmo;

		[SerializeField]
		private int _maxAmmo;

		[SerializeField]
		private int _missileTimeToDetonate;

		[SerializeField]
		private int _missileForceImpulse;

		[SerializeField]
		private int _missileBounciness;

		[SerializeField]
		private int _splashRadius;

		[SerializeField]
		private int _projectilesPerShot;

		[SerializeField]
		private int _projectileSpeed;

		[SerializeField]
		private int _rateOfFire;

		[SerializeField]
		private int _recoilMovement;

		[SerializeField]
		private int _combatRange;

		[SerializeField]
		private int _tier;

		[SerializeField]
		private int _secondaryActionReticle;

		[SerializeField]
		private int _weaponSecondaryAction;

		private int _criticalStrikeBonus;

		[SerializeField]
		private bool _hasAutoFire;

		[SerializeField]
		private int _defaultZoomMultiplier;

		[SerializeField]
		private int _minZoomMultiplier;

		[SerializeField]
		private int _maxZoomMultiplier;
	}
}
