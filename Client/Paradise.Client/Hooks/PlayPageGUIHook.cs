using HarmonyLib;
using log4net;
using System.Collections.Generic;
using System.Reflection;
using UberStrike.Core.Models;
using UnityEngine;

namespace Paradise.Client {
	[HarmonyPatch(typeof(PlayPageGUI))]
	public class PlayPageGUIHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(PlayPageGUIHook));
		private static ParadiseTraverse<PlayPageGUI> traverse;

		private static float nextServerCheckTime;

		static PlayPageGUIHook() {
			Log.Info($"[{nameof(PlayPageGUIHook)}] hooking {nameof(PlayPageGUI)}");
		}

		[HarmonyPatch("DrawQuickSearch"), HarmonyPrefix]
		public static bool PlayPageGUI_DrawQuickSearch_Prefix(PlayPageGUI __instance, Rect rect) {
			bool enabled = GUI.enabled;
			GUI.enabled = (enabled && Time.time > nextServerCheckTime);

			if (GUITools.Button(new Rect(rect.x - (8 + 64), rect.y, 64, rect.height), new GUIContent((nextServerCheckTime >= Time.time) ? $"{LocalizedStrings.Refresh} ({nextServerCheckTime - Time.time:N0})" : LocalizedStrings.Refresh), BlueStonez.buttondark_medium)) {
				Singleton<GameStateController>.Instance.Client.RefreshGameLobby();
				nextServerCheckTime = Time.time + 10f;
			}
			GUI.enabled = enabled;

			var searchBar = typeof(PlayPageGUI).GetField("_searchBar", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

			AccessTools.TypeByName("SearchBarGUI").GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance).Invoke(searchBar, new object[] { rect });

			return false;
		}

		[HarmonyPatch("DrawAllGames"), HarmonyPrefix]
		public static bool PlayPageGUI_DrawAllGames_Prefix(PlayPageGUI __instance, ref int __result, Rect rect, bool hasVScroll) {
			if (traverse == null) {
				traverse = ParadiseTraverse<PlayPageGUI>.Create(__instance);
			}

			var _cachedGameList = traverse.GetField<List<GameRoomData>>("_cachedGameList");

			var playerLevel = PlayerDataManager.PlayerLevel;
			var list = new List<string>();

			var index = 0;
			foreach (GameRoomData gameRoomData in _cachedGameList) {
				if ((bool)traverse.InvokeMethod("CanPassFilter", gameRoomData)) {
					var canJoinGame = GameRoomHelper.CanJoinGame(gameRoomData);
					var enabled = GUI.enabled;
					GUI.enabled = (enabled && canJoinGame && traverse.GetField<int>("_dropDownList") == 0);

					int yPos = 70 * index - 1;
					GUI.Box(new Rect(0f, yPos, rect.width, 71f), new GUIContent(string.Empty), BlueStonez.box_grey50);

					if (!ApplicationDataManager.IsMobile) {
						var tooltip = LocalizedStrings.PlayCaps;

						if (!GameRoomHelper.IsLevelAllowed(gameRoomData, playerLevel) && gameRoomData.LevelMin > playerLevel) {
							tooltip = string.Format(LocalizedStrings.YouHaveToReachLevelNToJoinThisGame, gameRoomData.LevelMin);
						} else if (!GameRoomHelper.IsLevelAllowed(gameRoomData, playerLevel) && gameRoomData.LevelMax < playerLevel) {
							tooltip = string.Format(LocalizedStrings.YouAlreadyMasteredThisLevel, new object[0]);
						} else if (gameRoomData.IsFull) {
							tooltip = string.Format(LocalizedStrings.ThisGameIsFull, new object[0]);
						}

						GUI.Box(new Rect(0f, yPos, rect.width, 70f), new GUIContent(string.Empty, tooltip), BlueStonez.box_grey50);
					}

					var _selectedGame = traverse.GetField<GameRoomData>("_selectedGame");
					if (_selectedGame != null && _selectedGame.Number == gameRoomData.Number) {
						GUI.color = new Color(1f, 1f, 1f, 0.03f);
						GUI.DrawTexture(new Rect(1f, yPos, rect.width + 1f, 70f), UberstrikeIconsHelper.White);
						GUI.color = Color.white;
					}

					GUIStyle style = (!canJoinGame) ? BlueStonez.label_interparkmed_10pt_left : BlueStonez.label_interparkbold_11pt_left;
					GUI.color = ((!GameRoomHelper.HasLevelRestriction(gameRoomData)) ? Color.white : new Color(1f, 0.7f, 0f));

					int xPos = 0;
					var item = (string)traverse.InvokeMethod("DisplayMapIcon", gameRoomData.MapID, new Rect(xPos, yPos, 110f, 70f));
					list.Add(item);

					if (gameRoomData.IsPermanentGame && GameRoomHelper.HasLevelRestriction(gameRoomData)) {
						if (gameRoomData.LevelMax <= 5) {
							GUI.DrawTexture(new Rect(80f, (yPos + 70 - 30), 25f, 25f), traverse.GetField<Texture2D>("_level1GameIcon"));
						} else if (gameRoomData.LevelMax <= 10) {
							GUI.DrawTexture(new Rect(80f, (yPos + 70 - 30), 25f, 25f), traverse.GetField<Texture2D>("_level2GameIcon"));
						} else if (gameRoomData.LevelMax <= 20) {
							GUI.DrawTexture(new Rect(80f, (yPos + 70 - 30), 25f, 25f), traverse.GetField<Texture2D>("_level3GameIcon"));
						} else if (gameRoomData.LevelMin >= 40) {
							GUI.DrawTexture(new Rect(80f, (yPos + 70 - 30), 25f, 25f), traverse.GetField<Texture2D>("_level20GameIcon"));
						} else if (gameRoomData.LevelMin >= 30) {
							GUI.DrawTexture(new Rect(80f, (yPos + 70 - 30), 25f, 25f), traverse.GetField<Texture2D>("_level10GameIcon"));
						} else if (gameRoomData.LevelMin >= 20) {
							GUI.DrawTexture(new Rect(80f, (yPos + 70 - 30), 25f, 25f), traverse.GetField<Texture2D>("_level5GameIcon"));
						}

						if (playerLevel > gameRoomData.LevelMax) {
							GUI.DrawTexture(new Rect(0f, (yPos + 70 - 50), 50f, 50f), UberstrikeIcons.LevelMastered);
						}
					}

					if (gameRoomData.IsPasswordProtected) {
						GUI.DrawTexture(new Rect(80f, (yPos + 70 - 30), 25f, 25f), traverse.GetField<Texture2D>("_privateGameIcon"));
					}

					GUI.color = ((!GameRoomHelper.HasLevelRestriction(gameRoomData)) ? Color.white : new Color(1f, 0.7f, 0f));

					xPos = 120;
					var _gameNameWidth = traverse.GetField<int>("_gameNameWidth");
					var _gameModeWidth = traverse.GetField<int>("_gameModeWidth");

					GUI.Label(new Rect(xPos, yPos, _gameNameWidth, 35f), gameRoomData.Name, BlueStonez.label_interparkbold_13pt_left);
					GUI.Label(new Rect(xPos, (yPos + 35), _gameNameWidth, 35f), Singleton<MapManager>.Instance.GetMapName(gameRoomData.MapID) + " " + (string)traverse.InvokeMethod("LevelRestrictionText", gameRoomData), BlueStonez.label_interparkmed_10pt_left);
					xPos = 122 + _gameNameWidth - 4;

					var timeLimit = gameRoomData.TimeLimit / 60;

					GUI.Label(new Rect(xPos, yPos, _gameModeWidth, 17f), string.Concat(new object[] {
						AccessTools.TypeByName("GameStateHelper").GetMethod("GetModeName").Invoke(null, new object[] { gameRoomData.GameMode }),
						" - ",
						timeLimit == 0 ? "No time limit" : $"{timeLimit} mins"
					}));

					GUI.Label(new Rect(xPos, yPos + 17f, _gameModeWidth, 17f), $"Mods: {PlayPageGUI.GetGameFlagText(gameRoomData)}");

					GUI.Label(new Rect((xPos + 64), (yPos + 35), _gameModeWidth, 35f), string.Format("{0}/{1} players", gameRoomData.ConnectedPlayers, gameRoomData.PlayerLimit), style);
					GUI.color = Color.white;

					traverse.InvokeMethod("DrawProgressBarLarge", new Rect(xPos, (yPos + 35 + 5), 58f, 35f), gameRoomData.ConnectedPlayers / (float)gameRoomData.PlayerLimit);

					xPos = 110 + _gameNameWidth + _gameModeWidth - 6;
					var height = BlueStonez.button.normal.background.height;

					if (GUI.Button(new Rect(xPos, (yPos + 35 - height / 2), 90f, height), LocalizedStrings.JoinCaps, BlueStonez.button_white)) {
						traverse.InvokeMethod("JoinGame", gameRoomData);
						AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.JoinServer, 0UL, 1f, 1f);
					}

					if (GUI.Button(new Rect(0f, yPos, rect.width, 70f), string.Empty, BlueStonez.label_interparkbold_11pt_left)) {
						Singleton<GameStateController>.Instance.Client.RefreshGameLobby();

						if (_selectedGame != null && _selectedGame.Number == gameRoomData.Number && traverse.GetField<float>("_gameJoinDoubleClick") > Time.time) {
							traverse.SetField("_gameJoinDoubleClick", 0f);
							traverse.InvokeMethod("JoinGame", _selectedGame);
							AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.JoinServer, 0UL, 1f, 1f);
						} else {
							traverse.SetField("_gameJoinDoubleClick", 0f);
						}

						traverse.SetField("_selectedGame", gameRoomData);
					}

					index++;

					GUI.color = Color.white;
					GUI.enabled = enabled;
				}
			}

			if (index == 0 && Singleton<GameServerController>.Instance.SelectedServer != null && Singleton<GameServerController>.Instance.SelectedServer.Data.RoomsCreated > 0 && _cachedGameList.Count > 0) {
				GUI.Label(new Rect(0f, rect.height * 0.5f, rect.width, 23f), "No games running on this server", BlueStonez.label_interparkmed_11pt);

				if (GUITools.Button(new Rect(rect.width * 0.5f - 70f, rect.height * 0.5f - 30f, 140f, 23f), new GUIContent(LocalizedStrings.CreateGameCaps), BlueStonez.button)) {
					PanelManager.Instance.OpenPanel(PanelType.CreateGame);
				}
			}

			__result = index;

			return false;
		}
	}
}
