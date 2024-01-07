using System.Collections.Generic;
using System.Text;
using UberStrike.Realtime.Client;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugTrafficPanel : IDebugPage {
		public string Title => "Traffic";

		private readonly string[] tabs = { "Comm", "Game" };
		private int selectedTab;


		private bool enableMonitoring;
		private Vector2 consoleScrollPos;
		private bool autoScroll = true;

		public DebugTrafficPanel() {
			AutoMonoBehaviour<CommConnectionManager>.Instance.Client.Monitor.AddNamesForPeerOperations(typeof(ICommPeerOperationsType));
			AutoMonoBehaviour<CommConnectionManager>.Instance.Client.Monitor.AddNamesForRoomOperations(typeof(ILobbyRoomOperationsType));

			Singleton<GameStateController>.Instance.Client.Monitor.AddNamesForPeerOperations(typeof(IGamePeerOperationsType));
			Singleton<GameStateController>.Instance.Client.Monitor.AddNamesForRoomOperations(typeof(IGameRoomOperationsType));
		}

		public void Draw() {
			ParadiseGUITools.DrawGroup("Traffic Log", delegate {
				selectedTab = GUILayout.Toolbar(selectedTab, tabs, BlueStonez.tab_medium);
				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				var _enableMonitoring = GUILayout.Toggle(enableMonitoring, "Enable Monitoring", BlueStonez.toggle);
				if (_enableMonitoring != enableMonitoring) {
					enableMonitoring = _enableMonitoring;

					ParadiseTraverse.Create(AutoMonoBehaviour<CommConnectionManager>.Instance.Client.Monitor).SetProperty("IsEnabled", enableMonitoring);
					ParadiseTraverse.Create(Singleton<GameStateController>.Instance.Client.Monitor).SetProperty("IsEnabled", enableMonitoring);

					if (!enableMonitoring) {
						ClearEvents();
					}
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				autoScroll = GUILayout.Toggle(autoScroll, "Enable Auto Scroll", BlueStonez.toggle);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Clear", BlueStonez.buttondark_small, GUILayout.Width(64f), GUILayout.Height(22f))) {
					ClearEvents();
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUI.enabled = enableMonitoring;
				consoleScrollPos = GUILayout.BeginScrollView(consoleScrollPos, false, true, GUIStyle.none, BlueStonez.verticalScrollbar, BlueStonez.scrollView);

				switch (selectedTab) {
					case 0:
						GUILayout.TextArea(Debug(AutoMonoBehaviour<CommConnectionManager>.Instance.Client.Monitor.AllEvents), BlueStonez.textArea);
						break;
					case 1:
						GUILayout.TextArea(Debug(Singleton<GameStateController>.Instance.Client.Monitor.AllEvents), BlueStonez.textArea);
						break;
				}

				if (autoScroll) consoleScrollPos.y = float.MaxValue;

				GUILayout.EndScrollView();
				GUI.enabled = true;
			});



		}

		private string Debug(LinkedList<string> list) {
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string value in list) {
				stringBuilder.AppendLine(value);
			}
			return stringBuilder.ToString();
		}

		private void ClearEvents() {
			AutoMonoBehaviour<CommConnectionManager>.Instance.Client.Monitor.AllEvents.Clear();
			Singleton<GameStateController>.Instance.Client.Monitor.AllEvents.Clear();
		}
	}
}
