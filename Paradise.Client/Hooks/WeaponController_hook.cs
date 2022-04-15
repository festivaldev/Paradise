﻿using HarmonyLib;
using System.Reflection;
using UberStrike.Realtime.UnitySdk;

namespace Paradise.Client {
	public static class WeaponController_hook {
		public static WeaponController Instance;

		public static void Hook() {
			var harmony = new Harmony("tf.festival.Paradise.WeaponController_hook");

			var orig_WeaponController_Shoot = typeof(WeaponController).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public);
			var prefix_WeaponController_Shoot = typeof(WeaponController_hook).GetMethod("Shoot_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_WeaponController_Shoot = typeof(WeaponController_hook).GetMethod("Shoot_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmony.Patch(orig_WeaponController_Shoot, new HarmonyMethod(prefix_WeaponController_Shoot), new HarmonyMethod(postfix_WeaponController_Shoot));
		}

		public static bool Shoot_Prefix(WeaponController __instance) {
			if (Instance == null) {
				Instance = __instance;
			}

			return true;
		}

		public static void Shoot_Postfix(ref bool __result) {
			UnityEngine.Debug.Log("game flag test");
			if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.QuickSwitch, GameState.Current.RoomData.GameFlags)) {
				UnityEngine.Debug.Log("game flag is set");
				typeof(WeaponController).GetField("_holsterTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Instance, 0);
			}
		}
	}
}
