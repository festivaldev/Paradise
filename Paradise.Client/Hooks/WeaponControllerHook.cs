using HarmonyLib;
using System.Reflection;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

namespace Paradise.Client {
	public class WeaponControllerHook : IParadiseHook {
		private static WeaponController WeaponControllerInstance;

		public void Hook(Harmony harmonyInstance) {
			Debug.Log($"[{typeof(WeaponControllerHook)}] hooking {typeof(WeaponController)}");

			var orig_WeaponController_Shoot = typeof(WeaponController).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public);
			var prefix_WeaponController_Shoot = typeof(WeaponControllerHook).GetMethod("Shoot_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_WeaponController_Shoot = typeof(WeaponControllerHook).GetMethod("Shoot_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_WeaponController_Shoot, new HarmonyMethod(prefix_WeaponController_Shoot), new HarmonyMethod(postfix_WeaponController_Shoot));
		}

		public static bool Shoot_Prefix(WeaponController __instance) {
			if (WeaponControllerInstance == null) {
				WeaponControllerInstance = __instance;
			}

			return true;
		}

		public static void Shoot_Postfix(ref bool __result) {
			if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.QuickSwitch, GameState.Current.RoomData.GameFlags)) {
				typeof(WeaponController).GetField("_holsterTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(WeaponControllerInstance, 0);
			}
		}
	}
}
