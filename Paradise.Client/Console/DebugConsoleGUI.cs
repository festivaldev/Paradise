using Cmune.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client {
	class DebugConsoleGUI : MonoBehaviour {
		public static DebugConsoleGUI Instance { get; private set; }
		public bool IsDebugConsoleEnabled { get; set; }

		private static IDebugPage _currentPageSelected;
		private static int _currentPageSelectedIdx = 0;
		private static string[] _debugPageDescriptors = new string[0];
		private static IDebugPage[] _debugPages = new IDebugPage[0];
		private List<string> _exceptions = new List<string>(10);
		private Vector2 _scrollDebug;

		void OnGUI() {
			if (IsDebugConsoleEnabled) {
				DrawDebugMenuGrid();
				DrawDebugPage();
			}
		}

		private void Awake() {
			Instance = this;

			_debugPages = new IDebugPage[] {
				new DebugGameObjects(),
				new DebugAnimation(),
				new DebugApplication(),
				new DebugGames(),
				new DebugGameServerManager(),
				new DebugGameState(),
				new DebugGraphics(),
				new DebugLogMessages(),
				new DebugMaps(),
				new DebugPlayerManager(),
				new DebugPlayersInGame(),
				new DebugProjectiles(),
				new DebugServerState(),
				new DebugShop(),
				new DebugSpawnPoints(),
				new DebugTraffic(),
				new DebugWebServices()
			};

			_debugPageDescriptors = new string[_debugPages.Length];

			for (int i = 0; i < _debugPages.Length; i++) {
				_debugPageDescriptors[i] = _debugPages[i].Title;
			}

			_currentPageSelectedIdx = 0;
			_currentPageSelected = _debugPages[0];
		}

		private void DrawDebugMenuGrid() {
			int num = GUILayout.SelectionGrid(
				selected: _currentPageSelectedIdx,
				texts: _debugPageDescriptors,
				xCount: 8,
				BlueStonez.tab_medium,
				GUILayout.MaxWidth(Math.Min(800, Screen.width))
			);

			if (num != _currentPageSelectedIdx) {
				num = Mathf.Clamp(num, 0, _debugPages.Length - 1);
				_currentPageSelectedIdx = num;
				_currentPageSelected = _debugPages[num];
			}
		}

		private void DrawDebugPage() {
			_scrollDebug = GUILayout.BeginScrollView(_scrollDebug, new GUILayoutOption[0]);
			if (_currentPageSelected != null) {
				_currentPageSelected.Draw();
			}
			GUILayout.EndScrollView();
		}

		private void Update() {
			if (KeyInput.AltPressed && KeyInput.CtrlPressed && KeyInput.GetKeyDown(KeyCode.D)) {
				if (!Application.isEditor && PlayerDataManager.AccessLevel >= MemberAccessLevel.Default) {
					IsDebugConsoleEnabled = !IsDebugConsoleEnabled;
				}
			}
		}
	}
}
