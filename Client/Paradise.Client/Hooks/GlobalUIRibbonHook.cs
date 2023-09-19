using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Adds additional entries to the global ribbon menu
	/// </summary>
	[HarmonyPatch(typeof(GlobalUIRibbon))]
	public class GlobalUIRibbonHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(GlobalUIRibbonHook));

		private static ParadiseTraverse traverse;

		static GlobalUIRibbonHook() {
			Log.Info($"[{nameof(GlobalUIRibbonHook)}] hooking {nameof(GlobalUIRibbon)}");
		}

		[HarmonyPatch("InitOptionsDropdown"), HarmonyPostfix]
		public static void GlobalUIRibbon_InitOptionsDropdown_Postfix(GlobalUIRibbon __instance) {
			traverse = ParadiseTraverse.Create(__instance);
			var optionsDropdown = traverse.GetField<GuiDropDown>("_optionsDropdown");

			// Link to Discord Server
			optionsDropdown.Add(new GUIContent(" Discord", AutoMonoBehaviour<TextureLoader>.Instance.Load(ApplicationDataManager.ImagePath + "discord.png", null).Texture), delegate () {
				Application.OpenURL("discord:///channels/1071142989579178116/");
			});

			// Link to GitHub Issues
			optionsDropdown.Add(new GUIContent(" Report Issue", AutoMonoBehaviour<TextureLoader>.Instance.Load(ApplicationDataManager.ImagePath + "github.png", null).Texture), delegate () {
				Application.OpenURL("https://github.com/festivaldev/Paradise/issues");
			});

			// Debug Console
			EventHandler.Global.AddListener<GlobalEvents.Login>(delegate (GlobalEvents.Login e) {
				if (e.AccessLevel == MemberAccessLevel.Admin) {
					optionsDropdown.Add(new GUIContent(" CONSOLE"), delegate {
						//GameObject.Find("Plugin Holder").GetComponent<ConsolePanelGUI>().Show();
						AutoMonoBehaviour<ConsolePanelGUI>.Instance.Show();
					});
				}
			});
		}

		[HarmonyPatch("DoMenuBar"), HarmonyPostfix]
		public static void GlobalUIRibbon_DoMenuBar_Postfix(Rect rect) {
			if (!ApplicationDataManager.IsMobile) {
				if (GamePageManager.HasPage || GameState.Current.HasJoinedGame) {
					if (GameState.Current.GameMode != UberStrike.Core.Types.GameModeType.None) {
						var xOffset = 88f;
						var width = 100f;

						if (GamePageManager.HasPage) {
							xOffset = 420f;
						}

						if (GUITools.Button(new Rect(rect.width - (xOffset + width), rect.y + 9f, width, 26f), new GUIContent("Copy game link", "Copy this game's link to your clipboard"), BlueStonez.buttondark_medium)) {
							var room = GameState.Current.RoomData;

							TextEditor editor = new TextEditor {
								content = new GUIContent($"uberstrike://connect/{room.Server.ConnectionString}/{room.Number}")
							};

							editor.SelectAll();
							editor.Copy();
						}
					}
				}
			}
		}
	}
}
