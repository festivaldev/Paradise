using HarmonyLib;
using System.Reflection;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	public class HUDStatusPanelHook : IParadiseHook {
		private static HUDStatusPanel HUDStatusPanelInstance;

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

		public void Hook(Harmony harmonyInstance) {
			Debug.Log($"[{typeof(HUDStatusPanelHook)}] hooking {typeof(HUDStatusPanel)}");

			var orig_HUDStatusPanel_GetRemainingKillString = typeof(HUDStatusPanel).GetMethod("GetRemainingKillString", BindingFlags.Instance | BindingFlags.NonPublic);
			var prefix_HUDStatusPanel_GetRemainingKillString = typeof(HUDStatusPanelHook).GetMethod("GetRemainingKillString_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_HUDStatusPanel_GetRemainingKillString = typeof(HUDStatusPanelHook).GetMethod("GetRemainingKillString_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_HUDStatusPanel_GetRemainingKillString, new HarmonyMethod(prefix_HUDStatusPanel_GetRemainingKillString), new HarmonyMethod(postfix_HUDStatusPanel_GetRemainingKillString));

			Debug.Log($"[{typeof(HUDStatusPanelHook)}] hooking {typeof(HUDNotifications)}");

			var orig_HUDNotifications_GetGameModeHint = typeof(HUDNotifications).GetMethod("GetGameModeHint", BindingFlags.Instance | BindingFlags.NonPublic);
			var postfix_HUDNotifications_GetGameModeHint = typeof(HUDStatusPanelHook).GetMethod("GetGameModeHint_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_HUDNotifications_GetGameModeHint, null, new HarmonyMethod(postfix_HUDNotifications_GetGameModeHint));

			var type = typeof(HUDStatusPanel).Assembly.GetType("WaitingForPlayersState");
			Debug.Log($"[{typeof(HUDStatusPanelHook)}] hooking {type}");

			var orig_WaitingForPlayersState_OnUpdate = type.GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.Public);
			var prefix_WaitingForPlayersState_OnUpdate = typeof(HUDStatusPanelHook).GetMethod("OnUpdate_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_WaitingForPlayersState_OnUpdate = typeof(HUDStatusPanelHook).GetMethod("OnUpdate_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_WaitingForPlayersState_OnUpdate, new HarmonyMethod(prefix_WaitingForPlayersState_OnUpdate), new HarmonyMethod(postfix_WaitingForPlayersState_OnUpdate));
		}

		public static bool GetRemainingKillString_Prefix(HUDStatusPanel __instance) {
			if (HUDStatusPanelInstance == null) {
				HUDStatusPanelInstance = __instance;
			}

			return true;
		}

		public static void GetRemainingKillString_Postfix(int remainingKills, ref string __result) {
			var type = typeof(HUDStatusPanel);

			var GetRemainingRoundsString_method = type.GetMethod("GetRemainingRoundsString", BindingFlags.Instance | BindingFlags.NonPublic);
			__result = (string)GetRemainingRoundsString_method.Invoke(HUDStatusPanelInstance, new object[] { remainingKills });
		}

		public static void GetGameModeHint_Postfix(ref string __result) {
			__result = GetGamemodeHint(GameState.Current.GameMode);
		}

		public static bool OnUpdate_Prefix() {
			return false;
		}

		public static void OnUpdate_Postfix() {
			var v = GetGamemodeHint(GameState.Current.GameMode);

			GameData.Instance.OnNotificationFull.Fire(LocalizedStrings.WaitingForOtherPlayers, v, 0f);
		}
	}
}
