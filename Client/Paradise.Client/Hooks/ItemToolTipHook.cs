using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Adds human-readable statistics values to item tooltips in the Shop GUI
	/// </summary>
	[HarmonyPatch(typeof(ItemToolTip))]
	public class ItemToolTipHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ItemToolTipHook));

		private static ParadiseTraverse traverse;

		private static readonly FloatPropertyBar _ammo = new FloatPropertyBar(LocalizedStrings.Ammo);
		private static readonly FloatPropertyBar _damage = new FloatPropertyBar(LocalizedStrings.Damage);
		private static readonly FloatPropertyBar _fireRate = new FloatPropertyBar(LocalizedStrings.RateOfFire);
		private static readonly FloatPropertyBar _accuracy = new FloatPropertyBar(LocalizedStrings.Accuracy);
		private static readonly FloatPropertyBar _velocity = new FloatPropertyBar(LocalizedStrings.Velocity);
		private static readonly FloatPropertyBar _damageRadius = new FloatPropertyBar(LocalizedStrings.Radius);
		private static readonly FloatPropertyBar _armorCarried = new FloatPropertyBar(LocalizedStrings.ArmorCarried);
		private static int _criticalHit;
		private static int _startAmmo;

		static ItemToolTipHook() {
			Log.Info($"[{nameof(ItemToolTipHook)}] hooking {nameof(ItemToolTip)}");
		}

		[HarmonyPatch("OnGUI"), HarmonyPrefix]
		public static bool ItemToolTip_OnGUI_Prefix() {
			if (ConsolePanelGUI.IsOpen) {
				GUI.depth = -150;
			}

			return true;
		}

		[HarmonyPatch("SetItem"), HarmonyPostfix]
		public static void ItemToolTip_SetItem_Postfix(ItemToolTip __instance, IUnityItem item) {
			if (!ParadiseClient.Settings.ShowDetailedItemStatistics) return;

			if (traverse == null) {
				traverse = ParadiseTraverse.Create(__instance);
			}

			switch (item.View.ItemClass) {
				case UberstrikeItemClass.WeaponMelee: {
					if (item.View is UberStrikeItemWeaponView uberStrikeItemWeaponView) {
						_damage.Value = WeaponConfigurationHelper.GetDamage(uberStrikeItemWeaponView);
						_damage.Max = WeaponConfigurationHelper.MaxDamage;
						_fireRate.Value = WeaponConfigurationHelper.GetRateOfFire(uberStrikeItemWeaponView);
						_fireRate.Max = WeaponConfigurationHelper.MaxRateOfFire;
					}

					break;
				}
				case (UberstrikeItemClass)2: // Handguns
				case UberstrikeItemClass.WeaponMachinegun:
				case UberstrikeItemClass.WeaponShotgun:
				case UberstrikeItemClass.WeaponSniperRifle: {
					// Fix for handguns not showing any statistics
					traverse.SetField("OnDrawItemDetails", new Action(delegate {
						traverse.InvokeMethod("DrawInstantHitWeapon");
					}));

					if (item.View is UberStrikeItemWeaponView uberStrikeItemWeaponView2) {
						_ammo.Value = WeaponConfigurationHelper.GetAmmo(uberStrikeItemWeaponView2);
						_ammo.Max = WeaponConfigurationHelper.MaxAmmo;
						_startAmmo = uberStrikeItemWeaponView2.StartAmmo;
						_damage.Value = WeaponConfigurationHelper.GetDamage(uberStrikeItemWeaponView2);
						_damage.Max = WeaponConfigurationHelper.MaxDamage;
						_fireRate.Value = WeaponConfigurationHelper.GetRateOfFire(uberStrikeItemWeaponView2);
						_fireRate.Max = WeaponConfigurationHelper.MaxRateOfFire;
						_accuracy.Value = WeaponConfigurationHelper.MaxAccuracySpread - WeaponConfigurationHelper.GetAccuracySpread(uberStrikeItemWeaponView2);
						_accuracy.Max = WeaponConfigurationHelper.MaxAccuracySpread;

						if (item.View.ItemProperties.ContainsKey(ItemPropertyType.CritDamageBonus)) {
							_criticalHit = item.View.ItemProperties[ItemPropertyType.CritDamageBonus];
						} else {
							_criticalHit = 0;
						}
					}

					break;
				}
				case UberstrikeItemClass.WeaponCannon:
				case UberstrikeItemClass.WeaponSplattergun:
				case UberstrikeItemClass.WeaponLauncher: {
					if (item.View is UberStrikeItemWeaponView uberStrikeItemWeaponView3) {
						_ammo.Value = WeaponConfigurationHelper.GetAmmo(uberStrikeItemWeaponView3);
						_ammo.Max = WeaponConfigurationHelper.MaxAmmo;
						_startAmmo = uberStrikeItemWeaponView3.StartAmmo;
						_damage.Value = WeaponConfigurationHelper.GetDamage(uberStrikeItemWeaponView3);
						_damage.Max = WeaponConfigurationHelper.MaxDamage;
						_fireRate.Value = WeaponConfigurationHelper.GetRateOfFire(uberStrikeItemWeaponView3);
						_fireRate.Max = WeaponConfigurationHelper.MaxRateOfFire;
						_velocity.Value = WeaponConfigurationHelper.GetProjectileSpeed(uberStrikeItemWeaponView3);
						_velocity.Max = WeaponConfigurationHelper.MaxProjectileSpeed;
						_damageRadius.Value = WeaponConfigurationHelper.GetSplashRadius(uberStrikeItemWeaponView3);
						_damageRadius.Max = WeaponConfigurationHelper.MaxSplashRadius;
					}

					break;
				}
				case UberstrikeItemClass.GearBoots:
				case UberstrikeItemClass.GearHead:
				case UberstrikeItemClass.GearFace:
				case UberstrikeItemClass.GearUpperBody:
				case UberstrikeItemClass.GearLowerBody:
				case UberstrikeItemClass.GearGloves:
				case UberstrikeItemClass.GearHolo:
					_armorCarried.Value = ((UberStrikeItemGearView)item.View).ArmorPoints;
					_armorCarried.Max = 200f;

					break;
				case UberstrikeItemClass.QuickUseGeneral:
				case UberstrikeItemClass.QuickUseGrenade:
				case UberstrikeItemClass.QuickUseMine:
					break;
				default: break;
			}
		}

		[HarmonyPatch("DrawGear"), HarmonyPrefix]
		public static bool ItemToolTip_DrawGear_Prefix() {
			if (!ParadiseClient.Settings.ShowDetailedItemStatistics) return true;

			ProgressBar(new Rect(20f, 120f, 200f, 12f), _armorCarried.Title, _armorCarried.Percent, ColorScheme.ProgressBar, $"{_armorCarried.Value:F0} AP");

			return false;
		}

		[HarmonyPatch("DrawProjectileWeapon"), HarmonyPrefix]
		public static bool ItemToolTip_DrawProjectileWeapon_Prefix() {
			if (!ParadiseClient.Settings.ShowDetailedItemStatistics) return true;

			var isDragging = Singleton<DragAndDrop>.Instance.IsDragging && ShopUtils.IsProjectileWeapon(Singleton<DragAndDrop>.Instance.DraggedItem.Item) && Singleton<DragAndDrop>.Instance.DraggedItem.Item.View.ItemClass == traverse.GetField<IUnityItem>("_item").View.ItemClass;

			ProgressBar(new Rect(20f, 120f, 200f, 12f), _damage.Title, _damage.Percent, ColorScheme.ProgressBar, $"{_damage.Value:F0}");
			ProgressBar(new Rect(20f, 135f, 200f, 12f), _fireRate.Title, 1f - _fireRate.Percent, ColorScheme.ProgressBar, $"{_fireRate.Value:F1}");
			ProgressBar(new Rect(20f, 150f, 200f, 12f), _velocity.Title, _velocity.Percent, ColorScheme.ProgressBar, $"{_velocity.Value:F1} m/s");
			ProgressBar(new Rect(20f, 165f, 200f, 12f), _damageRadius.Title, _damageRadius.Percent, ColorScheme.ProgressBar, $"{_damageRadius.Value:F2} m");
			ProgressBar(new Rect(20f, 180f, 200f, 12f), _ammo.Title, _ammo.Percent, ColorScheme.ProgressBar, $"{_startAmmo:F0}/{_ammo.Value:F0}");

			if (isDragging) {
				UberStrikeItemWeaponView view = Singleton<DragAndDrop>.Instance.DraggedItem.Item.View as UberStrikeItemWeaponView;
				ComparisonOverlay(new Rect(20f, 120f, 200f, 12f), _damage.Percent, WeaponConfigurationHelper.GetDamageNormalized(view));
				ComparisonOverlay(new Rect(20f, 135f, 200f, 12f), 1f - _fireRate.Percent, 1f - WeaponConfigurationHelper.GetRateOfFireNormalized(view));
				ComparisonOverlay(new Rect(20f, 150f, 200f, 12f), _velocity.Percent, WeaponConfigurationHelper.GetProjectileSpeedNormalized(view));
				ComparisonOverlay(new Rect(20f, 165f, 200f, 12f), _damageRadius.Percent, WeaponConfigurationHelper.GetSplashRadiusNormalized(view));
			}

			return false;
		}

		[HarmonyPatch("DrawInstantHitWeapon"), HarmonyPrefix]
		public static bool ItemToolTip_DrawInstantHitWeapon_Prefix() {
			if (!ParadiseClient.Settings.ShowDetailedItemStatistics) return true;

			bool isDragging = Singleton<DragAndDrop>.Instance.IsDragging && ShopUtils.IsInstantHitWeapon(Singleton<DragAndDrop>.Instance.DraggedItem.Item) && Singleton<DragAndDrop>.Instance.DraggedItem.Item.View.ItemClass == traverse.GetField<IUnityItem>("_item").View.ItemClass;

			ProgressBar(new Rect(20f, 120f, 200f, 12f), _damage.Title, _damage.Percent, ColorScheme.ProgressBar, $"{_damage.Value:F0}");
			ProgressBar(new Rect(20f, 135f, 200f, 12f), _fireRate.Title, 1f - _fireRate.Percent, ColorScheme.ProgressBar, $"{_fireRate.Value:F1}");
			ProgressBar(new Rect(20f, 150f, 200f, 12f), _accuracy.Title, _accuracy.Percent, ColorScheme.ProgressBar, CmunePrint.Percent(_accuracy.Value / _accuracy.Max));
			ProgressBar(new Rect(20f, 165f, 200f, 12f), _ammo.Title, _ammo.Percent, ColorScheme.ProgressBar, $"{_startAmmo:F0}/{_ammo.Value:F0}");

			if (isDragging) {
				UberStrikeItemWeaponView view = Singleton<DragAndDrop>.Instance.DraggedItem.Item.View as UberStrikeItemWeaponView;
				ComparisonOverlay(new Rect(20f, 120f, 200f, 12f), _damage.Percent, WeaponConfigurationHelper.GetDamageNormalized(view));
				ComparisonOverlay(new Rect(20f, 135f, 200f, 12f), 1f - _fireRate.Percent, 1f - WeaponConfigurationHelper.GetRateOfFireNormalized(view));
				ComparisonOverlay(new Rect(20f, 150f, 200f, 12f), _accuracy.Percent, 1f - WeaponConfigurationHelper.GetAccuracySpreadNormalized(view));
			}

			return false;
		}

		[HarmonyPatch("DrawMeleeWeapon"), HarmonyPrefix]
		public static bool ItemToolTip_DrawMeleeWeapon_Prefix() {
			if (!ParadiseClient.Settings.ShowDetailedItemStatistics) return true;

			ProgressBar(new Rect(20f, 120f, 200f, 12f), _damage.Title, _damage.Percent, ColorScheme.ProgressBar, $"{_damage.Value:F0}");
			ProgressBar(new Rect(20f, 135f, 200f, 12f), _fireRate.Title, 1f - _fireRate.Percent, ColorScheme.ProgressBar, $"{_fireRate.Value:F1}");

			if (Singleton<DragAndDrop>.Instance.IsDragging && ShopUtils.IsMeleeWeapon(Singleton<DragAndDrop>.Instance.DraggedItem.Item)) {
				UberStrikeItemWeaponView view = Singleton<DragAndDrop>.Instance.DraggedItem.Item.View as UberStrikeItemWeaponView;
				ComparisonOverlay(new Rect(20f, 120f, 200f, 12f), _damage.Percent, WeaponConfigurationHelper.GetDamageNormalized(view));
				ComparisonOverlay(new Rect(20f, 135f, 200f, 12f), 1f - _fireRate.Percent, 1f - WeaponConfigurationHelper.GetRateOfFireNormalized(view));
			}

			return false;
		}

		[HarmonyPatch("DrawQuickItem"), HarmonyPrefix]
		public static bool ItemToolTip_DrawQuickItem_Prefix() {
			if (!ParadiseClient.Settings.ShowDetailedItemStatistics) return true;

			var item = traverse.GetField<IUnityItem>("_item");

			if (item != null) {
				var quickItemView = item.View as UberStrikeItemQuickView;

				var itemInShop = Singleton<ItemManager>.Instance.GetItemInShop(quickItemView.ID);
				if (itemInShop != null) {
					if (!itemInShop.Prefab) {
						var gameObject = itemInShop.Create(Vector3.zero, Quaternion.identity).GetComponent<QuickItem>();
						for (int i = 0; i < gameObject.transform.childCount; i++) {
							gameObject.transform.GetChild(i).gameObject.SetActive(false);
						}
					}
				}

				var quickItemConfiguration = itemInShop.Prefab.GetComponent<QuickItem>().Configuration;

				if (quickItemConfiguration is HealthBuffConfiguration) {
					var healthBuffConfiguration = quickItemConfiguration as HealthBuffConfiguration;

					GUI.Label(new Rect(20f, 102f, 200f, 20f), $"{LocalizedStrings.HealthColon} {healthBuffConfiguration.GetHealthBonusDescription()}", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 117f, 200f, 20f), $"{LocalizedStrings.TimeColon} {(healthBuffConfiguration.IncreaseTimes <= 0 ? LocalizedStrings.Instant : $"{(healthBuffConfiguration.IncreaseFrequency * healthBuffConfiguration.IncreaseTimes) / 1000f}:F1")}s", BlueStonez.label_interparkbold_11pt_left);
				} else if (quickItemConfiguration is AmmoBuffConfiguration) {
					var ammoBuffConfiguration = quickItemConfiguration as AmmoBuffConfiguration;

					GUI.Label(new Rect(20f, 102f, 200f, 20f), $"{LocalizedStrings.AmmoColon} {ammoBuffConfiguration.GetAmmoBonusDescription()}", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 117f, 200f, 20f), $"{LocalizedStrings.TimeColon} {(ammoBuffConfiguration.IncreaseTimes <= 0 ? LocalizedStrings.Instant : $"{(ammoBuffConfiguration.IncreaseFrequency * ammoBuffConfiguration.IncreaseTimes) / 1000f}:F1")}s", BlueStonez.label_interparkbold_11pt_left);
				} else if (quickItemConfiguration is ArmorBuffConfiguration) {
					var armorBuffConfiguration = quickItemConfiguration as ArmorBuffConfiguration;

					GUI.Label(new Rect(20f, 102f, 200f, 20f), $"{LocalizedStrings.AmmoColon} {armorBuffConfiguration.GetArmorBonusDescription()}", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 117f, 200f, 20f), $"{LocalizedStrings.TimeColon} {(armorBuffConfiguration.IncreaseTimes <= 0 ? LocalizedStrings.Instant : $"{(armorBuffConfiguration.IncreaseFrequency * armorBuffConfiguration.IncreaseTimes) / 1000f}:F1")}s", BlueStonez.label_interparkbold_11pt_left);
				} else if (quickItemConfiguration is ExplosiveGrenadeConfiguration) {
					var explosiveGrenadeConfiguration = quickItemConfiguration as ExplosiveGrenadeConfiguration;

					GUI.Label(new Rect(20f, 102f, 200f, 20f), $"{LocalizedStrings.DamageColon} {explosiveGrenadeConfiguration.Damage}HP", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 117f, 200f, 20f), $"{LocalizedStrings.RadiusColon} {explosiveGrenadeConfiguration.SplashRadius}m", BlueStonez.label_interparkbold_11pt_left);
				} else if (quickItemConfiguration is SpringGrenadeConfiguration) {
					var springGrenadeConfiguration = quickItemConfiguration as SpringGrenadeConfiguration;

					GUI.Label(new Rect(20f, 102f, 200f, 20f), $"{LocalizedStrings.ForceColon} {springGrenadeConfiguration.Force}", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 117f, 200f, 20f), $"{LocalizedStrings.LifetimeColon} {springGrenadeConfiguration.LifeTime}s", BlueStonez.label_interparkbold_11pt_left);
				}

				if (quickItemConfiguration != null) {
					GUI.Label(new Rect(20f, 132f, 200f, 20f), $"{LocalizedStrings.WarmupColon} {(quickItemView.WarmUpTime <= 0 ? LocalizedStrings.Instant : $"{(quickItemView.WarmUpTime / 1000f):F1}s")}", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 147f, 200f, 20f), $"{LocalizedStrings.CooldownColon} {(quickItemView.CoolDownTime <= 0 ? LocalizedStrings.Instant : $"{(quickItemView.CoolDownTime / 1000f):F1}s")}", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 162f, 200f, 20f), $"{LocalizedStrings.UsesPerLifeColon} {(quickItemView.UsesPerLife <= 0 ? LocalizedStrings.Unlimited : quickItemView.UsesPerLife.ToString())}", BlueStonez.label_interparkbold_11pt_left);
					GUI.Label(new Rect(20f, 177f, 200f, 20f), $"{LocalizedStrings.UsesPerGameColon} {(quickItemView.UsesPerGame <= 0 ? LocalizedStrings.Unlimited : quickItemView.UsesPerGame.ToString())}", BlueStonez.label_interparkbold_11pt_left);
				}
			}

			return false;
		}

		private static void ComparisonOverlay(Rect position, float value, float otherValue) {
			traverse.InvokeMethod("ComparisonOverlay", position, value, otherValue);
		}

		private static void ProgressBar(Rect position, string text, float percentage, Color barColor, string value) {
			traverse.InvokeMethod("ProgressBar", position, text, percentage, barColor, value);
		}
	}

	public class FloatPropertyBar {
		private float _value;
		private float _lastValue;
		private float _max = 1f;
		private float _time;

		public FloatPropertyBar(string title) {
			Title = title;
		}

		public string Title { get; private set; }

		public float SmoothValue {
			get {
				return Mathf.Lerp(_lastValue, Value, (Time.time - _time) * 5f);
			}
		}

		public float Value {
			get {
				return _value;
			}
			set {
				_lastValue = _value;
				_time = Time.time;
				_value = value;
			}
		}

		public float Percent {
			get {
				return SmoothValue / Max;
			}
		}

		public float Max {
			get {
				return _max;
			}
			set {
				_max = Mathf.Max(value, 1f);
			}
		}
	}
}
