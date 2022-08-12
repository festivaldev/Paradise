using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using System;
using System.Reflection;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

namespace Paradise.Client {
	public class CreateGamePanelGUIHook : IParadiseHook {
		private static CreateGamePanelGUI Instance;

		public void Hook(Harmony harmonyInstance) {
			Debug.Log($"[{typeof(CreateGamePanelGUIHook)}] hooking {typeof(CreateGamePanelGUI)}");

			var orig_CreateGamePanelGUI_DrawGameConfiguration = typeof(CreateGamePanelGUI).GetMethod("DrawGameConfiguration", BindingFlags.Instance | BindingFlags.NonPublic);
			var prefix_CreateGamePanelGUI_DrawGameConfiguration = typeof(CreateGamePanelGUIHook).GetMethod("DrawGameConfiguration_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_CreateGamePanelGUI_DrawGameConfiguration = typeof(CreateGamePanelGUIHook).GetMethod("DrawGameConfiguration_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_CreateGamePanelGUI_DrawGameConfiguration, new HarmonyMethod(prefix_CreateGamePanelGUI_DrawGameConfiguration), new HarmonyMethod(postfix_CreateGamePanelGUI_DrawGameConfiguration));


			var orig_CreateGamePanelGUI_ValidateGamePassword = typeof(CreateGamePanelGUI).GetMethod("ValidateGamePassword", BindingFlags.Instance | BindingFlags.NonPublic);
			var prefix_CreateGamePanelGUI_ValidateGamePassword = typeof(CreateGamePanelGUIHook).GetMethod("ValidateGamePassword_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_CreateGamePanelGUI_ValidateGamePassword = typeof(CreateGamePanelGUIHook).GetMethod("ValidateGamePassword_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_CreateGamePanelGUI_ValidateGamePassword, new HarmonyMethod(prefix_CreateGamePanelGUI_ValidateGamePassword), new HarmonyMethod(postfix_CreateGamePanelGUI_ValidateGamePassword));
		}

		public static bool DrawGameConfiguration_Prefix(CreateGamePanelGUI __instance) {
			if (Instance == null) {
				Instance = __instance;

				SetField("_maxLevelCurrent", 100);
				SetField("_gameFlags", GameFlags.GAME_FLAGS.QuickSwitch);
			}

			return false;
		}

		public static void DrawGameConfiguration_Postfix(Rect rect) {
			if (GetProperty<bool>("IsModeSupported")) {
				MapSettings mapSettings = GetField<UberstrikeMap>("_mapSelected").View.Settings[GetField<SelectionGroup<GameModeType>>("_modeSelection").Current];

				if (ApplicationDataManager.IsMobile) {
					mapSettings.PlayersMax = Mathf.Min(mapSettings.PlayersMax, 6);
				}

				GUI.BeginGroup(rect);
				GUI.Label(new Rect(6f, 0f, 100f, 25f), LocalizedStrings.GameName, BlueStonez.label_interparkbold_18pt_left);

				if (PlayerDataManager.AccessLevel >= MemberAccessLevel.Moderator) {
					GUI.SetNextControlName("GameName");
					SetField("_gameName", GUI.TextField(new Rect(130f, 5f, GetField<float>("_textFieldWidth"), 19f), GetField<string>("_gameName"), 18, BlueStonez.textField));

					if (string.IsNullOrEmpty(GetField<string>("_gameName")) && !GUI.GetNameOfFocusedControl().Equals("GameName")) {
						GUI.color = new Color(1f, 1f, 1f, 0.3f);
						GUI.Label(new Rect(128f, 12f, 200f, 19f), LocalizedStrings.EnterGameName, BlueStonez.label_interparkmed_11pt_left);
						GUI.color = Color.white;
					}

					if (GetField<string>("_gameName").Length > 18) {
						SetField("_gameName", GetField<string>("_gameName").Remove(18));
					}
				} else {
					GUI.Label(new Rect(130f, 5f, GetField<float>("_textFieldWidth"), 19f), GetField<string>("_gameName"), BlueStonez.label);
				}

				GUI.Label(new Rect(130f + GetField<float>("_textFieldWidth") + 16f, 5f, 100f, 19f), "(" + GetField<string>("_gameName").Length + "/18)", BlueStonez.label_interparkbold_11pt_left);

				GUI.Label(new Rect(6f, 25f, 100f, 25f), LocalizedStrings.Password, BlueStonez.label_interparkbold_18pt_left);
				GUI.SetNextControlName("GamePasswd");

				SetField("_password", GUI.PasswordField(new Rect(130f, 28f, GetField<float>("_textFieldWidth"), 19f), GetField<string>("_password"), '*', 16));
				SetField("_password", GetField<string>("_password").Trim(new char[] {
					'\n'
				}));

				if (string.IsNullOrEmpty(GetField<string>("_password")) && !GUI.GetNameOfFocusedControl().Equals("GamePasswd")) {
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
					GUI.Label(new Rect(138f, 33f, 200f, 19f), "No password", BlueStonez.label_interparkmed_11pt_left);
					GUI.color = Color.white;
				}

				if (GetField<string>("_password").Length > 16) {
					SetField("_password", GetField<string>("_password").Remove(16));
				}

				GUI.Label(new Rect(130f + GetField<float>("_textFieldWidth") + 16f, 28f, 100f, 19f), "(" + GetField<string>("_password").Length + "/16)", BlueStonez.label_interparkbold_11pt_left);

				GUI.Label(new Rect(6f, 55f, 110f, 25f), LocalizedStrings.MaxPlayers, BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 60f, 33f, 15f), Mathf.RoundToInt((float)mapSettings.PlayersCurrent).ToString(), BlueStonez.label_dropdown);

				mapSettings.PlayersCurrent = ((!ApplicationDataManager.IsMobile) ? mapSettings.PlayersCurrent : Mathf.Clamp(mapSettings.PlayersCurrent, 0, 6));

				mapSettings.PlayersCurrent = (int)GUI.HorizontalSlider(new Rect(170f, 60f, GetField<float>("_sliderWidth"), 15f), (float)mapSettings.PlayersCurrent, (float)mapSettings.PlayersMin, (float)mapSettings.PlayersMax);

				int timeLimit = Mathf.RoundToInt((float)(mapSettings.TimeCurrent / 60));

				GUI.Label(new Rect(6f, 83f, 100f, 25f), LocalizedStrings.TimeLimit, BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 83f, 33f, 15f), timeLimit.ToString(), BlueStonez.label_dropdown);
				mapSettings.TimeCurrent = 60 * (int)GUI.HorizontalSlider(new Rect(170f, 86f, GetField<float>("_sliderWidth"), 15f), (float)timeLimit, (float)(mapSettings.TimeMin / 60), 10f);

				GUI.Label(new Rect(6f, 106f, 100f, 25f), GetField<SelectionGroup<GameModeType>>("_modeSelection").Current != GameModeType.EliminationMode ? LocalizedStrings.MaxKills : LocalizedStrings.MaxRounds, BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 106f, 33f, 15f), mapSettings.KillsCurrent.ToString(), BlueStonez.label_dropdown);

				mapSettings.KillsCurrent = (int)GUI.HorizontalSlider(new Rect(170f, 109f, GetField<float>("_sliderWidth"), 15f), (float)mapSettings.KillsCurrent, (float)mapSettings.KillsMin, (float)mapSettings.KillsMax);

				GUI.Label(new Rect(6f, 150f, 100f, 25f), "Min Level", BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 150f, 33f, 15f), (GetField<int>("_minLevelCurrent") != 1) ? GetField<int>("_minLevelCurrent").ToString() : "All", BlueStonez.label_dropdown);

				int minLevel = (int)GUI.HorizontalSlider(new Rect(170f, 153f, GetField<float>("_sliderWidth"), 15f), (float)GetField<int>("_minLevelCurrent"), 1f, 100f);

				if (minLevel != GetField<int>("_minLevelCurrent")) {
					SetField("_minLevelCurrent", minLevel);
					SetField("_maxLevelCurrent", Mathf.Clamp(GetField<int>("_maxLevelCurrent"), GetField<int>("_minLevelCurrent"), 100));
				}

				GUI.Label(new Rect(6f, 172f, 100f, 25f), "Max Level", BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 172f, 33f, 15f), (GetField<int>("_maxLevelCurrent") != 100) ? GetField<int>("_maxLevelCurrent").ToString() : "All", BlueStonez.label_dropdown);

				int maxLevel = (int)GUI.HorizontalSlider(new Rect(170f, 175f, GetField<float>("_sliderWidth"), 15f), (float)GetField<int>("_maxLevelCurrent"), 1f, 100f);

				if (maxLevel != GetField<int>("_maxLevelCurrent")) {
					SetField("_maxLevelCurrent", maxLevel);
					SetField("_minLevelCurrent", Mathf.Clamp(GetField<int>("_minLevelCurrent"), 1, GetField<int>("_maxLevelCurrent")));
				}

				if (!GameRoomHelper.IsLevelAllowed(GetField<int>("_minLevelCurrent"), GetField<int>("_maxLevelCurrent"), PlayerDataManager.PlayerLevel) && GetField<int>("_minLevelCurrent") > PlayerDataManager.PlayerLevel) {
					GUI.contentColor = Color.red;
					GUI.Label(new Rect(170f, 190f, GetField<float>("_sliderWidth"), 25f), "Minimum Level is too high for you!", BlueStonez.label_interparkbold_11pt);
					GUI.contentColor = Color.white;
				} else if (!GameRoomHelper.IsLevelAllowed(GetField<int>("_minLevelCurrent"), GetField<int>("_maxLevelCurrent"), PlayerDataManager.PlayerLevel) && GetField<int>("_maxLevelCurrent") < PlayerDataManager.PlayerLevel) {
					GUI.contentColor = Color.red;
					GUI.Label(new Rect(170f, 190f, GetField<float>("_sliderWidth"), 25f), "Maximum Level is too low for you!", BlueStonez.label_interparkbold_11pt);
					GUI.contentColor = Color.white;
				}

				GUI.Label(new Rect(6f, 216f, 100f, 25f), "Mods", BlueStonez.label_interparkbold_18pt_left);

				var flags = Enum.GetValues(typeof(GameFlags.GAME_FLAGS));
				for (var i = 1; i <= (int)flags.GetValue(flags.Length - 1); i *= 2) {
					var flag = (GameFlags.GAME_FLAGS)i;

					var gameFlags = GetField<GameFlags.GAME_FLAGS>("_gameFlags");
					var hasFlag = (gameFlags & flag) == flag;

					switch (flag) {
						case GameFlags.GAME_FLAGS.LowGravity:
							hasFlag = GUI.Toggle(new Rect(6f, 241f, GetField<float>("_sliderWidth"), 16f), hasFlag, "Low Gravity", BlueStonez.toggle);
							break;
						case GameFlags.GAME_FLAGS.NoArmor:
							GUI.enabled = false;
							hasFlag = GUI.Toggle(new Rect(170f, 241f, GetField<float>("_sliderWidth"), 16f), hasFlag, "No Armor", BlueStonez.toggle);
							GUI.enabled = true;
							break;
						case GameFlags.GAME_FLAGS.QuickSwitch:
							hasFlag = GUI.Toggle(new Rect(6f, 261f, GetField<float>("_sliderWidth"), 16f), hasFlag, "Quick Switching", BlueStonez.toggle);
							break;
						case GameFlags.GAME_FLAGS.MeleeOnly:
							GUI.enabled = false;
							hasFlag = GUI.Toggle(new Rect(170f, 261f, GetField<float>("_sliderWidth"), 16f), hasFlag, "Nelee Only", BlueStonez.toggle);
							GUI.enabled = true;
							break;
						default: break;
					}

					SetField("_gameFlags", !hasFlag ? (gameFlags & ~flag) : (gameFlags | flag));
				}

				GUI.EndGroup();
			} else {
				GUI.Label(rect, "Unsupported Game Mode!", BlueStonez.label_interparkbold_18pt);
			}
		}

		public static bool ValidateGamePassword_Prefix() {
			return false;
		}

		public static void ValidateGamePassword_Postfix(string psv, ref bool __result) {
			bool result = false;

			if (!string.IsNullOrEmpty(psv) && psv.Length <= 16) {
				result = true;
			}

			__result = result;
		}

		private static T GetField<T>(string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			return (T)typeof(CreateGamePanelGUI).GetField(fieldName, flags).GetValue(Instance);
		}

		private static void SetField(string fieldName, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			typeof(CreateGamePanelGUI).GetField(fieldName, flags).SetValue(Instance, value);
		}

		private static T GetProperty<T>(string propertyName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			return (T)typeof(CreateGamePanelGUI).GetProperty(propertyName, flags).GetValue(Instance, null);
		}

		private static void SetProperty(string propertyName, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			typeof(CreateGamePanelGUI).GetProperty(propertyName, flags).SetValue(Instance, value, null);
		}
	}
}
