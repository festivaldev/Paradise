using HarmonyLib;
using log4net;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class OptionsPanelGUIHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		private static OptionsPanelGUI Instance;

		private static float textureQuality;
		private static int vsync;
		private static int antiAliasing;

		/// <summary>
		/// <br>• Lets users select every resolution independent from resolutions supported by a player's screen (if smaller or equal sized)
		/// <br>• Activates fullscreen using a checkbox instead of the last available resolution
		/// </summary>
		public OptionsPanelGUIHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(OptionsPanelGUIHook)}] hooking {nameof(OptionsPanelGUI)}");

			var orig_OptionsPanelGUI_Awake = typeof(OptionsPanelGUI).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
			var prefix_OptionsPanelGUI_Awake = typeof(OptionsPanelGUIHook).GetMethod("Awake_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_OptionsPanelGUI_Awake = typeof(OptionsPanelGUIHook).GetMethod("Awake_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_OptionsPanelGUI_Awake, new HarmonyMethod(prefix_OptionsPanelGUI_Awake), new HarmonyMethod(postfix_OptionsPanelGUI_Awake));

			var orig_OptionsPanelGUI_DoVideoGroup = typeof(OptionsPanelGUI).GetMethod("DoVideoGroup", BindingFlags.Instance | BindingFlags.NonPublic);
			var prefix_OptionsPanelGUI_DoVideoGroup = typeof(OptionsPanelGUIHook).GetMethod("DoVideoGroup_Prefix", BindingFlags.Static | BindingFlags.Public);
			var postfix_OptionsPanelGUI_DoVideoGroup = typeof(OptionsPanelGUIHook).GetMethod("DoVideoGroup_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_OptionsPanelGUI_DoVideoGroup, new HarmonyMethod(prefix_OptionsPanelGUI_DoVideoGroup), new HarmonyMethod(postfix_OptionsPanelGUI_DoVideoGroup));
		}

		public static bool Awake_Prefix(OptionsPanelGUI __instance) {
			if (Instance == null) {
				Instance = __instance;
			}

			return true;
		}

		public static void Awake_Postfix() {
			List<string> list = new List<string>();
			foreach (Resolution resolution in ScreenResolutionManager.Resolutions) {
				list.Add($"{resolution.width} X {resolution.height}");
			}

			SetField("_screenResText", list.ToArray());
		}

		public static bool DoVideoGroup_Prefix(OptionsPanelGUI __instance) {
			textureQuality = GetField<float>("_textureQuality");
			vsync = GetField<int>("_vsync");
			antiAliasing = GetField<int>("_antiAliasing");

			return false;
		}

		public static void DoVideoGroup_Postfix() {
			GUI.skin = BlueStonez.Skin;

			Rect position = new Rect(1f, 1f, GetField<Rect>("_rect").width - 33f, GetField<Rect>("_rect").height - 55f - 47f);
			Rect contentRect = new Rect(0f, 0f, GetField<int>("_desiredWidth"), GetField<Rect>("_rect").height + 200f - 55f - 46f - 20f);
			int num = 10;
			int num2 = 150;

			float width = position.width - 8f - 8f - 20f;
			if (!Application.isWebPlayer || GetField<bool>("showResolutions")) {
				contentRect.height += (GetField<string[]>("_screenResText").Length * 16);
			}
			SetField("_scrollVideo", GUITools.BeginScrollView(position, GetField<Vector2>("_scrollVideo"), contentRect, false, false, true));
			GUI.enabled = true;
			int num4 = UnityGUI.Toolbar(new Rect(0f, 5f, position.width - 10f, 22f), GetField<int>("_currentQuality"), GetField<string[]>("qualitySet"), GetField<string[]>("qualitySet").Length, BlueStonez.tab_medium);
			if (num4 != GetField<int>("_currentQuality")) {
				InvokeMethod("SetCurrentQuality", new object[] { num4 });
				AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.ButtonClick, 0UL, 1f, 1f);
			}

			if (OptionsPanelGUI.HorizontalScrollbar(new Rect(8f, 30f, width, 30f), LocalizedStrings.TextureQuality, ref textureQuality, 0f, 5f)) {
				SetField("_textureQuality", textureQuality);

				SetField("graphicsChanged", true);
				InvokeMethod("SetCurrentQuality", new object[] { GetField<string[]>("qualitySet").Length - 1 });
			}

			if (OptionsPanelGUI.HorizontalGridbar(new Rect(8f, 60f, width, 30f), LocalizedStrings.VSync, ref vsync, GetField<string[]>("vsyncSet"))) {
				SetField("_vsync", vsync);

				SetField("graphicsChanged", true);
				InvokeMethod("SetCurrentQuality", new object[] { GetField<string[]>("qualitySet").Length - 1 });
			}

			if (OptionsPanelGUI.HorizontalGridbar(new Rect(8f, 90f, width, 30f), LocalizedStrings.AntiAliasing, ref antiAliasing, GetField<string[]>("antiAliasingSet"))) {
				SetField("_antiAliasing", antiAliasing);

				SetField("graphicsChanged", true);
				InvokeMethod("SetCurrentQuality", new object[] { GetField<string[]>("qualitySet").Length - 1 });
			}

			int yPos = 130;

			if (!ApplicationDataManager.IsMobile) {
				SetField("_postProcessing", GUI.Toggle(new Rect(8f, yPos, width, 30f), ApplicationDataManager.ApplicationOptions.VideoPostProcessing, LocalizedStrings.ShowPostProcessingEffects, BlueStonez.toggle));
				if (GetField<bool>("_postProcessing") != ApplicationDataManager.ApplicationOptions.VideoPostProcessing) {
					SetField("graphicsChanged", true);
					InvokeMethod("SetCurrentQuality", new object[] { GetField<string[]>("qualitySet").Length - 1 });
				}
				yPos += 30;
			}

			bool showFps = GUI.Toggle(new Rect(8f, yPos, width, 30f), ApplicationDataManager.ApplicationOptions.VideoShowFps, LocalizedStrings.ShowFPS, BlueStonez.toggle);
			if (showFps != ApplicationDataManager.ApplicationOptions.VideoShowFps) {
				ApplicationDataManager.ApplicationOptions.VideoShowFps = showFps;
				GameData.Instance.VideoShowFps.Fire();
			}

			yPos += 30;

			if (!Application.isWebPlayer) {
				int resolutionItemsHeight = GetField<string[]>("_screenResText").Length * 16 + 16 + 40;

				InvokeMethod("DrawGroupControl", new object[] { new Rect(8f, yPos, width, resolutionItemsHeight), LocalizedStrings.ScreenResolution, BlueStonez.label_group_interparkbold_18pt });

				GUI.BeginGroup(new Rect(8f, yPos, width, resolutionItemsHeight));
				GUI.changed = false;

				bool useFullscreen = GUI.Toggle(new Rect(10f, 20f, width, 30f), Screen.fullScreen, "Use Fullscreen", BlueStonez.toggle);
				if (useFullscreen != Screen.fullScreen) {
					Screen.fullScreen = useFullscreen;
				}

				Rect position2 = new Rect(10f, 40f, (num + num2 * 2), resolutionItemsHeight);
				int resolutionIndex = GUI.SelectionGrid(position2, ScreenResolutionManager.CurrentResolutionIndex, GetField<string[]>("_screenResText"), 1, BlueStonez.radiobutton);

				if (resolutionIndex != ScreenResolutionManager.CurrentResolutionIndex) {
					var resolution = ScreenResolutionManager.Resolutions[resolutionIndex];
					Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
				}

				GUI.EndGroup();
			}
			GUITools.EndScrollView();
		}

		private static T GetField<T>(string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			return GetField<T>(Instance, fieldName, flags);
		}

		private static void SetField(string fieldName, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			SetField(Instance, fieldName, value, flags);
		}

		private static object InvokeMethod(string methodName, object[] parameters, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			return InvokeMethod(Instance, methodName, parameters, flags);
		}
	}
}
