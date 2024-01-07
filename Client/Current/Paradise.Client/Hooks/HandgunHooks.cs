using HarmonyLib;
using log4net;
using System.Collections.Generic;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Collection of hooks to make handguns in the "Handguns" shop category usable again
	/// </summary>
	[HarmonyPatch]
	public class HandgunHooks {
		private static readonly ILog Log = LogManager.GetLogger(nameof(HandgunHooks));

		//[HarmonyPatch(typeof(AmmoDepot), "SetMaxAmmoForType"), HarmonyPrefix]
		public static bool AmmoDepot_SetMaxAmmoForType_Prefix(UberstrikeItemClass weaponClass, int maxAmmoCount) {
			if (PlayerDataManager.IsPlayerLoggedIn && weaponClass == (UberstrikeItemClass)2) {
				((Dictionary<AmmoType, int>)AccessTools.Property(typeof(AmmoDepot), "_maxAmmo").GetValue(null, null))[AmmoType.Machinegun] = maxAmmoCount;

				return false;
			}

			return true;
		}

		//[HarmonyPatch(typeof(AmmoDepot), "SetStartAmmoForType"), HarmonyPrefix]
		public static bool AmmoDepot_SetStartAmmoForType_Prefix(UberstrikeItemClass weaponClass, int startAmmoCount) {
			if (PlayerDataManager.IsPlayerLoggedIn && weaponClass == (UberstrikeItemClass)2) {
				((Dictionary<AmmoType, int>)AccessTools.Property(typeof(AmmoDepot), "_startAmmo").GetValue(null, null))[AmmoType.Machinegun] = startAmmoCount;

				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(AmmoDepot), "TryGetAmmoType"), HarmonyPrefix]
		public static bool AmmoDepot_TryGetAmmoType_Prefix(ref bool __result, UberstrikeItemClass item, ref AmmoType t) {
			if (item == (UberstrikeItemClass)2) {
				t = AmmoType.Machinegun;
				__result = true;

				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(AvatarAnimationController), "ChangeWeaponType"), HarmonyPrefix]
		public static bool AvatarAnimationController_ChangeWeaponType_Prefix(AvatarAnimationController __instance, UberstrikeItemClass itemClass) {
			if (__instance.Animator != null && itemClass == (UberstrikeItemClass)2) {
				ParadiseTraverse.SetField(__instance, "weaponSwitch", true);

				__instance.Animator.SetInteger((int)AccessTools.TypeByName("AvatarAnimationController+ControlFields").GetField("WeaponClass").GetValue(null), 2);

				return false;
			}

			return true;
		}

		//[HarmonyPatch(typeof(DefaultItemUtil), "GetDefaultWeaponView"), HarmonyPrefix]
		public static bool DefaultItemUtil_GetDefaultWeaponView_Prefix(ref UberStrikeItemWeaponView __result, UberstrikeItemClass itemClass) {
			if (itemClass == (UberstrikeItemClass)2) {
				// TODO: Complete weapon statistics
				__result = new UberStrikeItemWeaponView {

				};

				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(HUDReticleController), "Start"), HarmonyPrefix]
		public static bool HUDReticleController_Start_Prefix(HUDReticleController __instance) {
			ParadiseTraverse.GetField<Dictionary<UberstrikeItemClass, ReticleView>>(__instance, "reticles").Add((UberstrikeItemClass)2, ParadiseTraverse.GetField<ReticleView>(__instance, "machinegun"));

			return true;
		}

		[HarmonyPatch(typeof(ItemManager), "GetDefaultWeaponItem"), HarmonyPrefix]
		public static bool ItemManager_GetDefaultWeaponItem_Prefix(ref GameObject __result, UberstrikeItemClass itemClass) {
			if (itemClass == (UberstrikeItemClass)2) {
				WeaponItem weaponItem = UnityItemConfiguration.Instance.UnityItemsDefaultWeapons.Find((WeaponItem item) => item.name.Equals("Handgun"));
				__result = (!(weaponItem != null)) ? null : weaponItem.gameObject;

				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(ShopPageGUI), "Start"), HarmonyPostfix]
		public static void ShopPageGUI_Start_Postfix(ShopPageGUI __instance) {
			var typeSelection = new SelectionGroup<UberstrikeItemType>();

			typeSelection.Add(UberstrikeItemType.Weapon, new GUIContent(ShopIcons.WeaponItems, LocalizedStrings.Weapons));
			typeSelection.Add(UberstrikeItemType.Gear, new GUIContent(ShopIcons.GearItems, LocalizedStrings.Gear));
			typeSelection.Add(UberstrikeItemType.QuickUse, new GUIContent(ShopIcons.QuickItems, LocalizedStrings.QuickItems));
			typeSelection.Add(UberstrikeItemType.Functional, new GUIContent(ShopIcons.FunctionalItems, LocalizedStrings.FunctionalItems));
			typeSelection.OnSelectionChange += delegate (UberstrikeItemType itemType) {
				ParadiseTraverse.InvokeMethod(__instance, "UpdateItemFilter");
			};

			typeSelection.Select(UberstrikeItemType.Weapon);

			ParadiseTraverse.SetField(__instance, "_typeSelection", typeSelection);

			var weaponClassSelection = new SelectionGroup<UberstrikeItemClass>();

			weaponClassSelection.Add(UberstrikeItemClass.WeaponMelee, new GUIContent(ShopIcons.StatsMostWeaponSplatsMelee, LocalizedStrings.MeleeWeapons));
			weaponClassSelection.Add((UberstrikeItemClass)2, new GUIContent(ShopIcons.StatsMostWeaponSplatsHandgun, "Handguns"));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponMachinegun, new GUIContent(ShopIcons.StatsMostWeaponSplatsMachinegun, LocalizedStrings.Machineguns));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponShotgun, new GUIContent(ShopIcons.StatsMostWeaponSplatsShotgun, LocalizedStrings.Shotguns));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponSniperRifle, new GUIContent(ShopIcons.StatsMostWeaponSplatsSniperRifle, LocalizedStrings.SniperRifles));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponCannon, new GUIContent(ShopIcons.StatsMostWeaponSplatsCannon, LocalizedStrings.Cannons));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponSplattergun, new GUIContent(ShopIcons.StatsMostWeaponSplatsSplattergun, LocalizedStrings.Splatterguns));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponLauncher, new GUIContent(ShopIcons.StatsMostWeaponSplatsLauncher, LocalizedStrings.Launchers));

			weaponClassSelection.OnSelectionChange += delegate (UberstrikeItemClass itemClass) {
				ParadiseTraverse.InvokeMethod(__instance, "UpdateItemFilter");
			};

			weaponClassSelection.SetIndex(-1);

			ParadiseTraverse.SetField(__instance, "_weaponClassSelection", weaponClassSelection);

			ParadiseTraverse.InvokeMethod(__instance, "UpdateItemFilter");
		}

		[HarmonyPatch(typeof(ShopUtils), "IsInstantHitWeapon"), HarmonyPostfix]
		public static void ShopUtils_IsInstantHitWeapon_Postfix(ref bool __result, IUnityItem view) {
			__result = view != null && view.View != null && (__result || view.View.ItemClass == (UberstrikeItemClass)2);
		}

		[HarmonyPatch(typeof(UberstrikeIconsHelper), "GetIconForItemClass"), HarmonyPrefix]
		public static bool UberstrikeIconsHelper_GetItemForIconClass_Prefix(ref Texture2D __result, UberstrikeItemClass itemClass) {
			if (itemClass == (UberstrikeItemClass)2) {
				__result = ShopIcons.StatsMostWeaponSplatsHandgun;

				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(UnityItemConfiguration), "GetDefaultIcon"), HarmonyPrefix]
		public static bool UnityItemConfiguration_GetDefaultIcon_Prefix(UnityItemConfiguration __instance, ref Texture2D __result, UberstrikeItemClass itemClass) {
			if (itemClass == (UberstrikeItemClass)2) {
				__result = __instance.DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Handgun"));

				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(WeaponSlot), "CreateWeaponLogic"), HarmonyPrefix]
		public static bool WeaponSlot_CreateWeaponLogic_Prefix(WeaponSlot __instance, UberStrikeItemWeaponView view, IWeaponController controller) {
			if (view.ItemClass == (UberstrikeItemClass)2) {
				ParadiseTraverse.SetProperty(__instance, "Decorator", ParadiseTraverse.InvokeMethod(__instance, "InstantiateWeaponDecorator", view.ID));
				ParadiseTraverse.SetProperty(__instance, "Item", __instance.Decorator.GetComponent<WeaponItem>());

				if (view.ProjectilesPerShot > 1) {
					ParadiseTraverse.SetProperty(__instance, "Logic", new InstantMultiHitWeapon(__instance.Item, __instance.Decorator, view.ProjectilesPerShot, controller, view));
				} else {
					ParadiseTraverse.SetProperty(__instance, "Logic", new InstantHitWeapon(__instance.Item, __instance.Decorator, controller, view));
				}

				return false;
			}

			return true;
		}
	}
}
