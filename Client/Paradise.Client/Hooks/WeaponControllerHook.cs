using HarmonyLib;
using log4net;
using UberStrike.Realtime.UnitySdk;

namespace Paradise.Client {
	/// <summary>
	/// Enables weapon quick switching if the game's flag is set or the player is exploring maps.
	/// </summary>
	[HarmonyPatch(typeof(WeaponController))]
	public class WeaponControllerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(WeaponControllerHook));

		static WeaponControllerHook() {
			Log.Info($"[{nameof(WeaponControllerHook)}] hooking {nameof(WeaponController)}");
		}

		[HarmonyPatch("Shoot"), HarmonyPostfix]
		public static void WeaponController_Shoot_Postfix(WeaponController __instance, ref bool __result) {
			if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.QuickSwitch, GameState.Current.RoomData.GameFlags) ||
				GameState.Current.GameMode == UberStrike.Core.Types.GameModeType.None) {
				Traverse.Create(__instance).Field("_holsterTime").SetValue(0);
			}
		}
	}
}
