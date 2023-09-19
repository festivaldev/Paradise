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
	/// <br>• Allows selecting game flags ("Mods").</br>
	/// <br>• Increases maximum level to 100.</br>
	/// <br>• Increases maximum password length to 16.</br>
	/// </summary>
	[HarmonyPatch(typeof(CreateGamePanelGUI))]
	public class CreateGamePanelGUIHook {
		const int MAX_PASSWORD_LENGTH = 16;

		private static readonly ILog Log = LogManager.GetLogger(nameof(CreateGamePanelGUIHook));
		private static ParadiseTraverse traverse;

		static CreateGamePanelGUIHook() {
			Log.Info($"[{nameof(CreateGamePanelGUIHook)}] hooking {nameof(CreateGamePanelGUI)}");
		}

		[HarmonyPatch("Start"), HarmonyPostfix]
		public static void CreateGamePanelGUI_Start_Postfix(CreateGamePanelGUI __instance) {
			traverse = ParadiseTraverse.Create(__instance);

			traverse.SetField("_maxLevelCurrent", 100);
			traverse.SetField("_gameFlags", GameFlags.GAME_FLAGS.QuickSwitch);
		}

		[HarmonyPatch("DrawGameConfiguration"), HarmonyPrefix]
		public static bool CreateGamePanelGUI_DrawGameConfiguration_Prefix(CreateGamePanelGUI __instance, Rect rect) {
			if (traverse.GetProperty<bool>("IsModeSupported")) {
				MapSettings mapSettings = traverse.GetField<UberstrikeMap>("_mapSelected").View.Settings[traverse.GetField<SelectionGroup<GameModeType>>("_modeSelection").Current];

				if (ApplicationDataManager.IsMobile) {
					mapSettings.PlayersMax = Mathf.Min(mapSettings.PlayersMax, 6);
				}

				GUI.BeginGroup(rect);
				GUI.Label(new Rect(6f, 0f, 100f, 25f), LocalizedStrings.GameName, BlueStonez.label_interparkbold_18pt_left);

				if (PlayerDataManager.AccessLevel >= MemberAccessLevel.Moderator) {
					GUI.SetNextControlName("GameName");
					traverse.SetField("_gameName", GUI.TextField(new Rect(130f, 5f, traverse.GetField<float>("_textFieldWidth"), 19f), traverse.GetField<string>("_gameName"), 18, BlueStonez.textField));

					if (string.IsNullOrEmpty(traverse.GetField<string>("_gameName")) && !GUI.GetNameOfFocusedControl().Equals("GameName")) {
						GUI.color = new Color(1f, 1f, 1f, 0.3f);
						GUI.Label(new Rect(128f, 12f, 200f, 19f), LocalizedStrings.EnterGameName, BlueStonez.label_interparkmed_11pt_left);
						GUI.color = Color.white;
					}

					if (traverse.GetField<string>("_gameName").Length > 18) {
						traverse.SetField("_gameName", traverse.GetField<string>("_gameName").Remove(18));
					}
				} else {
					GUI.Label(new Rect(130f, 5f, traverse.GetField<float>("_textFieldWidth"), 19f), traverse.GetField<string>("_gameName"), BlueStonez.label);
				}

				GUI.Label(new Rect(130f + traverse.GetField<float>("_textFieldWidth") + 16f, 5f, 100f, 19f), $"({traverse.GetField<string>("_gameName").Length}/18)", BlueStonez.label_interparkbold_11pt_left);

				GUI.Label(new Rect(6f, 25f, 100f, 25f), LocalizedStrings.Password, BlueStonez.label_interparkbold_18pt_left);
				GUI.SetNextControlName("GamePasswd");

				traverse.SetField("_password", GUI.PasswordField(new Rect(130f, 28f, traverse.GetField<float>("_textFieldWidth"), 19f), traverse.GetField<string>("_password"), '*', MAX_PASSWORD_LENGTH));
				traverse.SetField("_password", traverse.GetField<string>("_password").Trim(new char[] { '\n' }));

				if (string.IsNullOrEmpty(traverse.GetField<string>("_password")) && !GUI.GetNameOfFocusedControl().Equals("GamePasswd")) {
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
					GUI.Label(new Rect(138f, 33f, 200f, 19f), "No password", BlueStonez.label_interparkmed_11pt_left);
					GUI.color = Color.white;
				}

				if (traverse.GetField<string>("_password").Length > MAX_PASSWORD_LENGTH) {
					traverse.SetField("_password", traverse.GetField<string>("_password").Remove(MAX_PASSWORD_LENGTH));
				}

				GUI.Label(new Rect(130f + traverse.GetField<float>("_textFieldWidth") + 16f, 28f, 100f, 19f), $"({traverse.GetField<string>("_password").Length}/{MAX_PASSWORD_LENGTH})", BlueStonez.label_interparkbold_11pt_left);

				GUI.Label(new Rect(6f, 55f, 110f, 25f), LocalizedStrings.MaxPlayers, BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 60f, 33f, 15f), Mathf.RoundToInt(mapSettings.PlayersCurrent).ToString(), BlueStonez.label_dropdown);

				mapSettings.PlayersCurrent = ((!ApplicationDataManager.IsMobile) ? mapSettings.PlayersCurrent : Mathf.Clamp(mapSettings.PlayersCurrent, 0, 6));

				mapSettings.PlayersCurrent = (int)GUI.HorizontalSlider(new Rect(170f, 60f, traverse.GetField<float>("_sliderWidth"), 15f), mapSettings.PlayersCurrent, mapSettings.PlayersMin, mapSettings.PlayersMax);

				int timeLimit = Mathf.RoundToInt(mapSettings.TimeCurrent / 60);

				GUI.Label(new Rect(6f, 83f, 100f, 25f), LocalizedStrings.TimeLimit, BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 83f, 33f, 15f), timeLimit == 0 ? "No" : timeLimit.ToString(), BlueStonez.label_dropdown);
				mapSettings.TimeCurrent = 60 * (int)GUI.HorizontalSlider(new Rect(170f, 86f, traverse.GetField<float>("_sliderWidth"), 15f), timeLimit, (mapSettings.TimeMin / 60) - 1, 10f);

				GUI.Label(new Rect(6f, 106f, 100f, 25f), traverse.GetField<SelectionGroup<GameModeType>>("_modeSelection").Current != GameModeType.EliminationMode ? LocalizedStrings.MaxKills : LocalizedStrings.MaxRounds, BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 106f, 33f, 15f), mapSettings.KillsCurrent.ToString(), BlueStonez.label_dropdown);

				mapSettings.KillsCurrent = (int)GUI.HorizontalSlider(new Rect(170f, 109f, traverse.GetField<float>("_sliderWidth"), 15f), mapSettings.KillsCurrent, mapSettings.KillsMin, mapSettings.KillsMax);

				GUI.Label(new Rect(6f, 150f, 100f, 25f), "Min Level", BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 150f, 33f, 15f), (traverse.GetField<int>("_minLevelCurrent") != 1) ? traverse.GetField<int>("_minLevelCurrent").ToString() : "All", BlueStonez.label_dropdown);

				int minLevel = (int)GUI.HorizontalSlider(new Rect(170f, 153f, traverse.GetField<float>("_sliderWidth"), 15f), traverse.GetField<int>("_minLevelCurrent"), 1f, 100f);

				if (minLevel != traverse.GetField<int>("_minLevelCurrent")) {
					traverse.SetField("_minLevelCurrent", minLevel);
					traverse.SetField("_maxLevelCurrent", Mathf.Clamp(traverse.GetField<int>("_maxLevelCurrent"), traverse.GetField<int>("_minLevelCurrent"), 100));
				}

				GUI.Label(new Rect(6f, 172f, 100f, 25f), "Max Level", BlueStonez.label_interparkbold_18pt_left);
				GUI.Label(new Rect(130f, 172f, 33f, 15f), (traverse.GetField<int>("_maxLevelCurrent") != 100) ? traverse.GetField<int>("_maxLevelCurrent").ToString() : "All", BlueStonez.label_dropdown);

				int maxLevel = (int)GUI.HorizontalSlider(new Rect(170f, 175f, traverse.GetField<float>("_sliderWidth"), 15f), traverse.GetField<int>("_maxLevelCurrent"), 1f, 100f);

				if (maxLevel != traverse.GetField<int>("_maxLevelCurrent")) {
					traverse.SetField("_maxLevelCurrent", maxLevel);
					traverse.SetField("_minLevelCurrent", Mathf.Clamp(traverse.GetField<int>("_minLevelCurrent"), 1, traverse.GetField<int>("_maxLevelCurrent")));
				}

				if (!GameRoomHelper.IsLevelAllowed(traverse.GetField<int>("_minLevelCurrent"), traverse.GetField<int>("_maxLevelCurrent"), PlayerDataManager.PlayerLevel) && traverse.GetField<int>("_minLevelCurrent") > PlayerDataManager.PlayerLevel) {
					GUI.contentColor = Color.red;
					GUI.Label(new Rect(170f, 190f, traverse.GetField<float>("_sliderWidth"), 25f), "Minimum Level is too high for you!", BlueStonez.label_interparkbold_11pt);
					GUI.contentColor = Color.white;
				} else if (!GameRoomHelper.IsLevelAllowed(traverse.GetField<int>("_minLevelCurrent"), traverse.GetField<int>("_maxLevelCurrent"), PlayerDataManager.PlayerLevel) && traverse.GetField<int>("_maxLevelCurrent") < PlayerDataManager.PlayerLevel) {
					GUI.contentColor = Color.red;
					GUI.Label(new Rect(170f, 190f, traverse.GetField<float>("_sliderWidth"), 25f), "Maximum Level is too low for you!", BlueStonez.label_interparkbold_11pt);
					GUI.contentColor = Color.white;
				}

				GUI.Label(new Rect(6f, 216f, 100f, 25f), "Mods", BlueStonez.label_interparkbold_18pt_left);

				var flags = Enum.GetValues(typeof(GameFlags.GAME_FLAGS));

				for (var i = 1; i <= (int)flags.GetValue(flags.Length - 1); i *= 2) {
					var flag = (GameFlags.GAME_FLAGS)i;

					var gameFlags = traverse.GetField<GameFlags.GAME_FLAGS>("_gameFlags");
					var hasFlag = (gameFlags & flag) == flag;

					switch (flag) {
						case GameFlags.GAME_FLAGS.LowGravity:
							hasFlag = GUI.Toggle(new Rect(6f, 241f, traverse.GetField<float>("_sliderWidth"), 16f), hasFlag, "Low Gravity", BlueStonez.toggle);
							break;
						case GameFlags.GAME_FLAGS.NoArmor:
							GUI.enabled = false;
							hasFlag = GUI.Toggle(new Rect(170f, 241f, traverse.GetField<float>("_sliderWidth"), 16f), hasFlag, "No Armor", BlueStonez.toggle);
							GUI.enabled = true;
							break;
						case GameFlags.GAME_FLAGS.QuickSwitch:
							hasFlag = GUI.Toggle(new Rect(6f, 261f, traverse.GetField<float>("_sliderWidth"), 16f), hasFlag, "Quick Switching", BlueStonez.toggle);
							break;
						case GameFlags.GAME_FLAGS.MeleeOnly:
							GUI.enabled = false;
							hasFlag = GUI.Toggle(new Rect(170f, 261f, traverse.GetField<float>("_sliderWidth"), 16f), hasFlag, "Nelee Only", BlueStonez.toggle);
							GUI.enabled = true;
							break;
						default: break;
					}

					traverse.SetField("_gameFlags", !hasFlag ? (gameFlags & ~flag) : (gameFlags | flag));
				}

				GUI.EndGroup();
			} else {
				GUI.Label(rect, "Unsupported Game Mode!", BlueStonez.label_interparkbold_18pt);
			}

			return false;
		}

		[HarmonyPatch("ValidateGamePassword"), HarmonyPrefix]
		public static bool CreateGamePanelGUI_ValidateGamePassword_Prefix(string psv, ref bool __result) {
			bool result = false;

			if (!string.IsNullOrEmpty(psv) && psv.Length <= MAX_PASSWORD_LENGTH) {
				result = true;
			}

			__result = result;

			return false;
		}
	}
}
