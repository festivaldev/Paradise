using HarmonyLib;
using log4net;
using UberStrike.Core.Types;

namespace Paradise.Client {
	/// <summary>
	/// <br>• Changes "n Kills Remaining" to "n Rounds Remaining" for Team Elimination.</br>
	/// <br>• Displays the appropriate gamemode hint in Team Elimination.</br>
	/// <br>• Allows the "n Kills/Rounds Remaining" label to be updated using the same value (thus preventing "STARTS IN" to be always displayed).</br>
	/// </summary>
	[HarmonyPatch(typeof(HUDStatusPanel))]
	public class HUDStatusPanelHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(HUDStatusPanelHook));

		static HUDStatusPanelHook() {
			Log.Info($"[{nameof(HUDStatusPanelHook)}] hooking {nameof(HUDStatusPanel)}");
		}

		[HarmonyPatch("GetRemainingKillString"), HarmonyPostfix]
		public static void GetRemainingKillString_Postfix(HUDStatusPanel __instance, int remainingKills, ref string __result) {
			if (GameState.Current.GameMode == GameModeType.EliminationMode) {
				__result = (string)AccessTools.Method(typeof(HUDStatusPanel), "GetRemainingRoundsString").Invoke(__instance, new object[] { remainingKills });
			}
		}

		[HarmonyPatch("set_KillsRemaining"), HarmonyPostfix]
		public static void set_KillsRemaining_Postfix(HUDStatusPanel __instance, int value) {
			Traverse.Create(__instance).Field<UILabel>("statusLabel").Value.text = (string)AccessTools.Method(typeof(HUDStatusPanel), "GetRemainingKillString").Invoke(__instance, new object[] { value });
		}

		[HarmonyPatch(typeof(HUDNotifications), "GetGameModeHint"), HarmonyPostfix]
		public static void HUDNotifications_GetGameModeHint_Postfix(ref string __result) {
			__result = GetGamemodeHint(GameState.Current.GameMode);
		}

		[HarmonyPatch("WaitingForPlayersState", "OnUpdate"), HarmonyPrefix]
		public static bool WaitingForPlayersState_OnUpdate_Prefix() {
			var v = GetGamemodeHint(GameState.Current.GameMode);

			GameData.Instance.OnNotificationFull.Fire(LocalizedStrings.WaitingForOtherPlayers, v, 0f);

			return false;
		}

		[HarmonyPatch("OnUpdateRemainingSeconds"), HarmonyPrefix]
		public static bool HUDStatusPanel_OnUpdateRemainingSeconds_Prefix() {
			if (GameState.Current.RoomData.TimeLimit == 0) return false;

			return true;
		}



		private static string GetGamemodeHint(GameModeType gameModeType) {
			switch (GameState.Current.GameMode) {
				case GameModeType.DeathMatch:
					return "Get as many kills as you can before the time runs out";
				case GameModeType.TeamDeathMatch:
					return "Get as many kills for your team as you can\nbefore the time runs out";
				case GameModeType.EliminationMode:
					return "Kill all members of the opposing team\nto win this round";
				default: return null;
			}
		}
	}
}
