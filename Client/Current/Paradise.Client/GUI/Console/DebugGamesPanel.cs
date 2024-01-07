using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugGamesPanel : IDebugPage {
		public string Title => "Games";

		private int selectedGame = -1;

		public void Draw() {
			ParadiseGUITools.DrawGroup("Games", delegate {
				if (!Singleton<GameStateController>.Instance.Client.IsConnected) {
					GUI.enabled = false;
					GUILayout.Label("You're not connected to a game server", BlueStonez.label_interparkbold_11pt_left);
					GUI.enabled = true;

					return;
				}

				if (!Singleton<GameStateController>.Instance.Client.IsConnectedToLobby) {
					GUI.contentColor = ColorScheme.UberStrikeYellow;
					GUILayout.Label("You're disconnected from the game lobby", BlueStonez.label_interparkbold_11pt_left);
					GUI.contentColor = Color.white;

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

					if (GUILayout.Button(LocalizedStrings.Refresh, BlueStonez.buttondark_medium, GUILayout.Height(22f))) {
						Singleton<GameStateController>.Instance.Client.RefreshGameLobby();
					}

					return;
				}

				foreach (var item in Singleton<GameListManager>.Instance.GameList.ToList().Select((x, i) => new { Value = x, Index = i })) {
					var gameRoomData = item.Value;
					var isSelectedGame = gameRoomData.Number == selectedGame;

					if (item.Index > 0) GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					GUILayout.BeginHorizontal();

					var isSelected = GUILayout.Toggle(isSelectedGame, string.Empty, BlueStonez.radiobutton, GUILayout.Width(16f), GUILayout.Height(22f));
					if (isSelected && isSelected != isSelectedGame) {
						selectedGame = gameRoomData.Number;
					}

					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_H);

					GUILayout.Label($"[{gameRoomData.Number}] {gameRoomData.Name} - {Singleton<MapManager>.Instance.GetMapWithId(gameRoomData.MapID).Name} ({gameRoomData.ConnectedPlayers}/{gameRoomData.PlayerLimit}) ({gameRoomData.TimeLimit / 60f:F0} min)", BlueStonez.label_interparkbold_11pt_left, GUILayout.ExpandWidth(true), GUILayout.Height(22f));

					GUILayout.EndHorizontal();
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				GUI.enabled = selectedGame >= 0;
				if (GUILayout.Button("Close", BlueStonez.buttondark_small, GUILayout.Width(36f), GUILayout.Height(20f))) {
					AccessTools.Method(typeof(GamePeer), "CloseGame").Invoke(Singleton<GameStateController>.Instance.Client, new object[] { selectedGame });
					selectedGame = -1;
				};
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			});
		}
	}
}
