using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client {
	[HarmonyPatch]
	internal class ConsolePanelGUI : AutoMonoBehaviour<ConsolePanelGUI> {

		public static bool IsOpen { get; private set; }
		private static readonly GuiDepth depth = (GuiDepth)(-97);

		private Rect _rect;

		IDebugPage selectedDebugPage;
		private static List<IDebugPage> debugPages = new List<IDebugPage> {
			new DebugAnimationPanel(),
			new DebugApplicationPanel(),
			new DebugAudioPanel(),
			new DebugGamesPanel(),
			new DebugGameServerManagerPanel(),
			new DebugGameStatePanel(),
			new DebugSystemPanel(),
			new DebugLogMessagesPanel(),
			new DebugMapsPanel(),
			new DebugPlayerManagerPanel(),
			new DebugProjectilesPanel(),
			new DebugServerStatePanel(),
			new DebugShopPanel(),
			new DebugSpawnPointsPanel(),
			new DebugTrafficPanel(),
			new DebugWebServicesPanel()
		};

		private Vector2 _scrollPos;
		private Vector2 _pageScrollPos;

		private static ParadiseTraverse mouseOrbitTraverse;

		static ConsolePanelGUI() {

		}

		public void Show() {
			IsOpen = true;

			GuiLockController.EnableLock(depth);
		}

		public void Hide() {
			IsOpen = false;

			GuiLockController.ReleaseLock(depth);
		}

		public void Awake() {
			if (PlayerDataManager.AccessLevel >= MemberAccessLevel.SeniorQA) {
				debugPages.Insert(0, new DebugGameObjectsPanel());
			}
		}

		public void OnGUI() {
			if (!IsOpen) return;
			if (debugPages == null) return;

			GUI.skin = BlueStonez.Skin;
			GUI.depth = (int)depth;

			//_rect = new Rect {
			//	x = PANEL_OFFSET,
			//	y = PANEL_OFFSET,
			//	width = Screen.width - (PANEL_OFFSET * 2),
			//	height = Screen.height - (PANEL_OFFSET * 2)
			//};

			_rect = new Rect {
				x = (Screen.width - Mathf.Min(Screen.width, ParadiseGUITools.CONSOLE_PANEL_WIDTH)) / 2,
				y = (Screen.height - Mathf.Min(Screen.height, ParadiseGUITools.CONSOLE_PANEL_HEIGHT)) / 2,
				width = Mathf.Min(Screen.width, ParadiseGUITools.CONSOLE_PANEL_WIDTH),
				height = Mathf.Min(Screen.height, ParadiseGUITools.CONSOLE_PANEL_HEIGHT)
			};

			GUI.BeginGroup(_rect, GUIContent.none, BlueStonez.window_standard_grey38);

			GUILayout.BeginVertical(GUILayout.Width(_rect.width), GUILayout.Height(_rect.height));
			GUILayout.Label("CONSOLE", BlueStonez.tab_strip, GUILayout.Height(ParadiseGUITools.PANEL_TITLE_HEIGHT));

			GUILayout.Space(22f); // tab_medium height

			GUILayout.BeginHorizontal();

			// Page Selector
			_scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, GUIStyle.none, BlueStonez.verticalScrollbar, BlueStonez.scrollView, GUILayout.Width(200f));
			foreach (var page in debugPages) {
				if (GUILayout.Toggle(selectedDebugPage == page, new GUIContent(page.Title), BlueStonez.tab_large_left, GUILayout.Height(35))) {
					if (selectedDebugPage != page) selectedDebugPage = page;

					if (GUI.changed) {
						GUI.changed = false;
						AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.ButtonClick);
						_pageScrollPos = Vector2.zero;
					}
				}
			}
			GUILayout.EndScrollView();

			GUILayout.BeginVertical();

			_pageScrollPos = GUILayout.BeginScrollView(_pageScrollPos, false, true, GUIStyle.none, BlueStonez.verticalScrollbar, BlueStonez.scrollView, GUILayout.Height(_rect.height - 56f - 48f));
			GUILayout.BeginHorizontal(BlueStonez.window_standard_grey38, GUILayout.Height(_rect.height - 56f - 48f));
			GUILayout.Space(ParadiseGUITools.PANEL_PADDING_H);
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical(GUILayout.MaxWidth(480));
			GUILayout.Space(ParadiseGUITools.PANEL_PADDING_V);

			try {
				selectedDebugPage?.Draw();
			} catch (Exception e) {
				Debug.LogError(e);
			}

			GUILayout.Space(ParadiseGUITools.PANEL_PADDING_V);
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.Space(ParadiseGUITools.PANEL_PADDING_H_SCROLLBAR);
			GUILayout.EndHorizontal();
			GUILayout.EndScrollView();

			// Button Strip
			GUILayout.Space(ParadiseGUITools.PANEL_BUTTON_PADDING_V);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(LocalizedStrings.OkCaps, BlueStonez.button, GUILayout.Width(120f), GUILayout.Height(ParadiseGUITools.PANEL_BUTTON_HEIGHT))) {
				Hide();
			}

			GUILayout.Space(ParadiseGUITools.PANEL_BUTTON_PADDING_H);
			GUILayout.EndHorizontal();

			GUILayout.Space(ParadiseGUITools.PANEL_BUTTON_PADDING_V);

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUI.EndGroup();
			GuiManager.DrawTooltip();
		}

		[HarmonyPatch(typeof(MouseOrbit), "LateUpdate"), HarmonyPrefix]
		public static bool MouseOrbit_LateUpdate_Prefix(MouseOrbit __instance) {
			if (IsOpen) {
				if (mouseOrbitTraverse == null) {
					mouseOrbitTraverse = ParadiseTraverse.Create(__instance);
				}

				mouseOrbitTraverse.SetField("listenToMouseUp", false);
				mouseOrbitTraverse.SetField("mouseAxisSpin", Vector2.zero);
			}

			return !IsOpen;
		}

		//[HarmonyPatch(typeof(UIEventReceiver), "OnClick"), HarmonyPrefix]
		//public static bool UIEventReceiver_OnClick_Prefix() {
		//	return !ShowPanel;
		//}

		//[HarmonyPatch(typeof(GlobalUIRibbon), "OnGUI"), HarmonyPrefix]
		//public static bool GlobalUIRibbon_OnGUI_Prefix() {
		//	return !ShowPanel;
		//}
	}
}
