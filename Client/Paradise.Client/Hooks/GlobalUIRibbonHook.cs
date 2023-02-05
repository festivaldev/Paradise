using HarmonyLib;
using log4net;
using System;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class GlobalUIRibbonHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Adds additional entries to the global ribbon menu
		/// </summary>
		public GlobalUIRibbonHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(GlobalUIRibbonHook)}] hooking {nameof(GlobalUIRibbon)}");

			var orig_GlobalUIRibbon_InitOptionsDropdown = typeof(GlobalUIRibbon).GetMethod("InitOptionsDropdown", BindingFlags.NonPublic | BindingFlags.Instance);
			var postfix_GlobalUIRibbon_InitOptionsDropdown = typeof(GlobalUIRibbonHook).GetMethod("InitOptionsDropdown_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_GlobalUIRibbon_InitOptionsDropdown, null, new HarmonyMethod(postfix_GlobalUIRibbon_InitOptionsDropdown));

			var orig_GlobalUIRibbon_DoMenuBar = typeof(GlobalUIRibbon).GetMethod("DoMenuBar", BindingFlags.NonPublic | BindingFlags.Instance);
			var postfix_GlobalUIRibbon_DoMenuBar = typeof(GlobalUIRibbonHook).GetMethod("DoMenuBar_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_GlobalUIRibbon_DoMenuBar, null, new HarmonyMethod(postfix_GlobalUIRibbon_DoMenuBar));
		}

		public static void InitOptionsDropdown_Postfix(GlobalUIRibbon __instance) {
			var optionsDropdown = GetField<GuiDropDown>(__instance, "_optionsDropdown");

			// Link to Discord Server
			optionsDropdown.Add(new GUIContent(" Discord", AutoMonoBehaviour<TextureLoader>.Instance.Load(ApplicationDataManager.ImagePath + "discord.png", null).Texture), delegate () {
				Application.OpenURL("discord:///channels/1071142989579178116/");
			});

			// Link to GitHub Issues
			optionsDropdown.Add(new GUIContent(" Report Issue", AutoMonoBehaviour<TextureLoader>.Instance.Load(ApplicationDataManager.ImagePath + "github.png", null).Texture), delegate () {
				Application.OpenURL("https://github.com/festivaldev/Paradise/issues");
			});
		}

		public static void DoMenuBar_Postfix(GlobalUIRibbon __instance, Rect rect) {
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

							TextEditor editor = new TextEditor();
							editor.content = new GUIContent($"uberstrike://connect/{room.Server.ConnectionString}/{room.Number}");
							editor.SelectAll();
							editor.Copy();
						}
					}
				}
			}
		}
	}
}
