using HarmonyLib;
using log4net;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// <br>• Removes "Sale Items" from Shop GUI</br>
	/// <br>• Adds "Handguns" back to the Shop GUI</br>
	/// </summary>
	[HarmonyPatch(typeof(ShopPageGUI))]
	public class ShopPageGUIHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ShopPageGUIHook));

		private static ParadiseTraverse traverse;

		static ShopPageGUIHook() {
			Log.Info($"[{nameof(ShopPageGUIHook)}] hooking {nameof(ShopPageGUI)}");
		}

		[HarmonyPatch("Start"), HarmonyPostfix]
		public static void ShopPageGUI_Start_Postfix(ShopPageGUI __instance) {
			traverse = ParadiseTraverse.Create(__instance);

			var typeSelection = new SelectionGroup<UberstrikeItemType>();

			typeSelection.Add(UberstrikeItemType.Weapon, new GUIContent(ShopIcons.WeaponItems, LocalizedStrings.Weapons));
			typeSelection.Add(UberstrikeItemType.Gear, new GUIContent(ShopIcons.GearItems, LocalizedStrings.Gear));
			typeSelection.Add(UberstrikeItemType.QuickUse, new GUIContent(ShopIcons.QuickItems, LocalizedStrings.QuickItems));
			typeSelection.Add(UberstrikeItemType.Functional, new GUIContent(ShopIcons.FunctionalItems, LocalizedStrings.FunctionalItems));
			typeSelection.OnSelectionChange += delegate (UberstrikeItemType itemType) {
				traverse.InvokeMethod("UpdateItemFilter");
			};

			typeSelection.Select(UberstrikeItemType.Weapon);

			traverse.SetField("_typeSelection", typeSelection);

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
				traverse.InvokeMethod("UpdateItemFilter");
			};

			weaponClassSelection.SetIndex(-1);

			traverse.SetField("_weaponClassSelection", weaponClassSelection);

			traverse.InvokeMethod("UpdateItemFilter");
		}
	}
}
