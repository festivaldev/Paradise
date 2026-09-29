/*
 * Copyright (C) 2017, 2022-2026 Team FESTIVAL
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// <br>• Changes "n Kills Remaining" to "n Rounds Remaining" for Team Elimination.</br>
	/// <br>• Displays the appropriate gamemode hint in Team Elimination.</br>
	/// <br>• Allows the "n Kills/Rounds Remaining" label to be updated using the same value (thus preventing "STARTS IN" to be always displayed).</br>
	/// <br>• Fixes the round timer displaying time and kills remaining from a previous round.</br>
	/// </summary>
	[HarmonyPatch(typeof(HUDStatusPanel))]
	public static class HUDStatusPanelHook {
		[HarmonyPatch("GetRemainingKillString"), HarmonyPostfix]
		public static void GetRemainingKillString_Postfix(HUDStatusPanel __instance, int remainingKills, ref string __result) {
			if (GameState.Current.GameMode == GameModeType.EliminationMode) {
				__result = (string)ParadiseTraverse.InvokeMethod(__instance, "GetRemainingRoundsString", remainingKills);
			}
		}

		[HarmonyPatch("set_KillsRemaining"), HarmonyPostfix]
		public static void set_KillsRemaining_Postfix(HUDStatusPanel __instance, int value) {
			ParadiseTraverse.GetField<UILabel>(__instance, "statusLabel").text = (string)ParadiseTraverse.InvokeMethod(__instance, "GetRemainingKillString", value);
		}

		[HarmonyPatch(typeof(HUDNotifications), "GetGameModeHint"), HarmonyPostfix]
		public static void HUDNotifications_GetGameModeHint_Postfix(ref string __result) {
			__result = GetGamemodeHint(GameState.Current.GameMode);
		}

		[HarmonyPatch("WaitingForPlayersState", "OnUpdate"), HarmonyPrefix]
		public static bool WaitingForPlayersState_OnUpdate_Prefix() {
			if (GameState.Current.PlayerState.CurrentStateId == PlayerStateId.Killed) {
				GameData.Instance.OnNotificationFull.Fire(string.Empty, string.Empty, 0f);
				return false;
			}

			var v = GetGamemodeHint(GameState.Current.GameMode);

			GameData.Instance.OnNotificationFull.Fire(LocalizedStrings.WaitingForOtherPlayers, v, 0f);

			return false;
		}

		[HarmonyPatch("OnUpdateRemainingSeconds"), HarmonyPrefix]
		public static bool HUDStatusPanel_OnUpdateRemainingSeconds_Prefix() {
			if (!GameState.Current.IsMatchRunning) return true;

			if (GameState.Current.RoomData.TimeLimit == 0)
				return false;

			return true;
		}

		[HarmonyPatch("Start"), HarmonyPostfix]
		public static void Start_Postfix(HUDStatusPanel __instance) {
			if (!GameState.Current.IsMatchRunning) {
				ParadiseTraverse.GetField<UILabel>(__instance, "statusLabel").text = (string)ParadiseTraverse.InvokeMethod(__instance, "GetRemainingKillString", GameState.Current.RoomData.KillLimit);
				ParadiseTraverse.GetField<UILabel>(__instance, "timerLabel").text = (string)ParadiseTraverse.InvokeMethod(__instance, "GetClockString", GameState.Current.RoomData.TimeLimit);
			}

			if (GameState.Current.RoomData.TimeLimit == 0) {
				ParadiseTraverse.InvokeMethod(__instance, "StopPulse");
			}
		}

		private static string GetGamemodeHint(GameModeType gameModeType) {
			switch (GameState.Current.GameMode) {
				case GameModeType.DeathMatch:
					return "Get as many kills as you can before the time runs out";
				case GameModeType.TeamDeathMatch:
					return "Get as many kills for your team as you can\nbefore the time runs out";
				case GameModeType.EliminationMode:
					return "Kill all members of the opposing team\nto win this round";
				default:
					return null;
			}
		}
	}
}
