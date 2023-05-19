using Cmune.DataCenter.Common.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Client {
	class DebugConsoleGUI : MonoBehaviour {
		public static bool Show;

		private int SelectedDebugPageIndex = -1;
		private IDebugPage SelectedDebugPage;

		private IDebugPage[] DebugPages = new IDebugPage[] {
			new DebugGameObjects(),
			new DebugAnimation(),
			new DebugApplication(),
			new DebugAudio(),
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
		private List<string> DebugPageDescriptors = new List<string>();

		private Vector2 ScrollPos;

		void Awake() {
			foreach (var page in DebugPages) {
				DebugPageDescriptors.Add(page.Title);
			}
		}

		void Update() {
			if (KeyInput.AltPressed && KeyInput.CtrlPressed && KeyInput.GetKeyDown(KeyCode.D)) {
				if (!Application.isEditor && PlayerDataManager.AccessLevel > MemberAccessLevel.Default || Show) {
					Show = !Show;

					AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(Show ? GameAudio.OpenPanel : GameAudio.ClosePanel, 0UL, 1f, 1f);
				}
			}
		}

		void OnGUI() {
			if (Show) {
				DrawDebugMenuGrid();
				DrawDebugPage();
			}
		}

		private void DrawDebugMenuGrid() {
			var selectedTab = GUILayout.SelectionGrid(
				selected: SelectedDebugPageIndex,
				texts: DebugPageDescriptors.ToArray(),
				xCount: DebugPages.Count(),
				BlueStonez.tab_medium,
				GUILayout.MaxWidth(Screen.width)
			);

			if (selectedTab != SelectedDebugPageIndex) {
				selectedTab = Mathf.Clamp(selectedTab, 0, DebugPages.Count() - 1);

				SelectedDebugPageIndex = selectedTab;
				SelectedDebugPage = DebugPages[selectedTab];

				AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.ButtonClick, 0UL, 1f, 1f);
			}
		}

		private void DrawDebugPage() {
			ScrollPos = GUILayout.BeginScrollView(ScrollPos, new GUILayoutOption[0]);

			SelectedDebugPage?.Draw();

			GUILayout.EndScrollView();
		}
	}
}
