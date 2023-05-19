using HarmonyLib;
using log4net;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// <br>• Lets users select every resolution independent from resolutions supported by a player's screen (if smaller or equal sized)
	/// <br>• Activates fullscreen using a checkbox instead of the last available resolution
	/// </summary>
	[HarmonyPatch(typeof(OptionsPanelGUI))]
	public class OptionsPanelGUIHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(OptionsPanelGUIHook));

		private static OptionsPanelGUI Instance;
		private static Traverse traverse;

		private static float textureQuality;
		private static int vsync;
		private static int antiAliasing;

		static OptionsPanelGUIHook() {
			Log.Info($"[{nameof(OptionsPanelGUIHook)}] hooking {nameof(OptionsPanelGUI)}");
		}

		[HarmonyPatch("Awake"), HarmonyPrefix]
		public static bool OptionsPanelGUI_Awake_Prefix(OptionsPanelGUI __instance) {
			if (Instance == null) {
				Instance = __instance;
				traverse = Traverse.Create(__instance);
			}

			return true;
		}

		[HarmonyPatch("Awake"), HarmonyPostfix]
		public static void OptionsPanelGUI_Awake_Postfix() {
			List<string> list = new List<string>();
			foreach (Resolution resolution in ScreenResolutionManager.Resolutions) {
				list.Add($"{resolution.width} X {resolution.height}");
			}

			SetField("_screenResText", list.ToArray());
		}

		[HarmonyPatch("DoVideoGroup"), HarmonyPrefix]
		public static bool OptionsPanelGUI_DoVideoGroup_Prefix() {
			textureQuality = GetField<float>("_textureQuality");
			vsync = GetField<int>("_vsync");
			antiAliasing = GetField<int>("_antiAliasing");

			return false;
		}

		[HarmonyPatch("DoVideoGroup"), HarmonyPostfix]
		public static void OptionsPanelGUI_DoVideoGroup_Postfix() {
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

			int toolbarIndex = UnityGUI.Toolbar(new Rect(0f, 5f, position.width - 10f, 22f), GetField<int>("_currentQuality"), GetField<string[]>("qualitySet"), GetField<string[]>("qualitySet").Length, BlueStonez.tab_medium);

			if (toolbarIndex != GetField<int>("_currentQuality")) {
				InvokeMethod("SetCurrentQuality", new object[] { toolbarIndex });
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

		[HarmonyPatch("DoControlsGroup"), HarmonyPrefix]
		public static bool OptionsPanelGUI_DoControlsGroup_Prefix() {
			return false;
		}

		[HarmonyPatch("DoControlsGroup"), HarmonyPostfix]
		public static void OptionsPanelGUI_DoControlsGroup_Postfix() {
			GUITools.PushGUIState();
			GUI.enabled = (GetField<UserInputMap>("_targetMap") == null);
			GUI.skin = BlueStonez.Skin;

			var _rect = GetField<Rect>("_rect");
			var _keyCount = GetField<int>("_keyCount");

			SetField("_scrollControls", GUITools.BeginScrollView(new Rect(1f, 3f, _rect.width - 33f, _rect.height - 55f - 50f), GetField<Vector2>("_scrollControls"), new Rect(0f, 0f, _rect.width - 50f, (float)(210 + _keyCount * 21)), false, false, true));

			InvokeMethod("DrawGroupControl", new Rect(8f, 20f, _rect.width - 65f, 65f), LocalizedStrings.Mouse, BlueStonez.label_group_interparkbold_18pt);
			GUI.BeginGroup(new Rect(8f, 20f, _rect.width - 65f, 65f));

			GUI.Label(new Rect(15f, 10f, 130f, 30f), LocalizedStrings.MouseSensitivity, BlueStonez.label_interparkbold_11pt_left);
			float mouseSensitivity = GUI.HorizontalSlider(new Rect(155f, 17f, 200f, 30f), ApplicationDataManager.ApplicationOptions.InputXMouseSensitivity, 0.1f, 10f, BlueStonez.horizontalSlider, BlueStonez.horizontalSliderThumb);
			GUI.Label(new Rect(370f, 10f, 100f, 30f), ApplicationDataManager.ApplicationOptions.InputXMouseSensitivity.ToString("N1"), BlueStonez.label_interparkbold_11pt_left);

			if (mouseSensitivity != ApplicationDataManager.ApplicationOptions.InputXMouseSensitivity) {
				ApplicationDataManager.ApplicationOptions.InputXMouseSensitivity = mouseSensitivity;
			}

			bool invertMouse = GUI.Toggle(new Rect(15f, 38f, 200f, 30f), ApplicationDataManager.ApplicationOptions.InputInvertMouse, LocalizedStrings.InvertMouseButtons, BlueStonez.toggle);
			if (invertMouse != ApplicationDataManager.ApplicationOptions.InputInvertMouse) {
				ApplicationDataManager.ApplicationOptions.InputInvertMouse = invertMouse;
			}

			GUI.EndGroup();

			int yPos = 105;

			if (Input.GetJoystickNames().Length > 0) {
				InvokeMethod("DrawGroupControl", new Rect(8f, 105f, _rect.width - 65f, 50f), LocalizedStrings.Gamepad, BlueStonez.label_group_interparkbold_18pt);
				GUI.BeginGroup(new Rect(8f, 105f, _rect.width - 65f, 50f));

				bool enableGamepad = GUI.Toggle(new Rect(15f, 15f, 400f, 30f), AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled, Input.GetJoystickNames()[0], BlueStonez.toggle);

				if (enableGamepad != AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled) {
					AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled = enableGamepad;
				}

				GUI.EndGroup();

				yPos += 70;
			} else if (AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled) {
				AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled = false;
			}

			InvokeMethod("DrawGroupControl", new Rect(8f, (float)yPos, _rect.width - 65f, (float)(_keyCount * 21 + 20)), LocalizedStrings.Keyboard, BlueStonez.label_group_interparkbold_18pt);
			GUI.BeginGroup(new Rect(8f, (float)yPos, _rect.width - 65f, (float)(_keyCount * 21 + 20)));

			InvokeMethod("DoInputControlMapping", new Rect(5f, 5f, _rect.width - 60f, (float)(_keyCount * 21 + 20)));

			GUI.EndGroup();
			GUITools.EndScrollView();
			GUITools.PopGUIState();
		}

		#region 
		private static T GetField<T>(string fieldName) {
			return traverse.Field<T>(fieldName).Value;
		}

		private static void SetField(string fieldName, object value) {
			traverse.Field(fieldName).SetValue(value);
		}

		private static T GetProperty<T>(string propertyName) {
			return traverse.Property<T>(propertyName).Value;
		}

		private static void SetProperty(string propertyName, object value) {
			traverse.Property(propertyName).SetValue(value);
		}

		private static object InvokeMethod(string methodName, params object[] parameters) {
			return AccessTools.Method(Instance.GetType(), methodName).Invoke(Instance, parameters);
		}
		#endregion
	}
}
