using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Adds additional entries to the global ribbon menu
	/// </summary>
	[HarmonyPatch(typeof(GlobalUIRibbon))]
	public class GlobalUIRibbonHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(GlobalUIRibbonHook));

		private static ParadiseTraverse<GlobalUIRibbon> traverse;

		static GlobalUIRibbonHook() {
			Log.Info($"[{nameof(GlobalUIRibbonHook)}] hooking {nameof(GlobalUIRibbon)}");
		}

		[HarmonyPatch("InitOptionsDropdown"), HarmonyPostfix]
		public static void GlobalUIRibbon_InitOptionsDropdown_Postfix(GlobalUIRibbon __instance) {
			traverse = ParadiseTraverse<GlobalUIRibbon>.Create(__instance);
			var optionsDropdown = traverse.GetField<GuiDropDown>("_optionsDropdown");

			// Link to Discord Server
			using (var stream = Assembly.GetAssembly(typeof(ParadiseClient)).GetManifestResourceStream("Paradise.Client.Resources.icon_discord.png")) {
				using (var reader = new BinaryReader(stream)) {
					var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
					texture.LoadImage(reader.ReadBytes((int)stream.Length));


					optionsDropdown.Add(new GUIContent(" Discord", texture), delegate {
						Application.OpenURL("discord:///channels/1071142989579178116/");
					});
				}
			}

			// Link to GitHub Issues
			using (var stream = Assembly.GetAssembly(typeof(ParadiseClient)).GetManifestResourceStream("Paradise.Client.Resources.icon_github.png")) {
				using (var reader = new BinaryReader(stream)) {
					var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
					texture.LoadImage(reader.ReadBytes((int)stream.Length));

					optionsDropdown.Add(new GUIContent(" Report Issue", texture), delegate {
						Application.OpenURL("https://github.com/festivaldev/Paradise/issues");
					});
				}
			}

			// Debug Console
			EventHandler.Global.AddListener<GlobalEvents.Login>(delegate (GlobalEvents.Login e) {
				if (e.AccessLevel == MemberAccessLevel.Admin) {
					optionsDropdown.Add(new GUIContent(" CONSOLE"), delegate {
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
