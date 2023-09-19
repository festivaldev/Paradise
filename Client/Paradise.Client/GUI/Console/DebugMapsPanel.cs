using System;
using System.Linq;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugMapsPanel : IDebugPage {
		public string Title => "Maps";

		public void Draw() {
			foreach (var item in Singleton<MapManager>.Instance.AllMaps.Select((x, i) => new { Value = x, Index = i })) {
				if (item.Index > 0) {
					GUILayout.Space(ParadiseGUITools.SECTION_SPACING);
				}

				var uberstrikeMap = item.Value;

				ParadiseGUITools.DrawGroup(uberstrikeMap.Name, delegate {
					ParadiseGUITools.DrawTextField("ID", uberstrikeMap.Id);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
					ParadiseGUITools.DrawTextField("Scene Name", uberstrikeMap.SceneName);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
					ParadiseGUITools.DrawTextField("Map Icon", uberstrikeMap.MapIconUrl);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
					ParadiseGUITools.DrawTextArea("Description", uberstrikeMap.Description);

					if (uberstrikeMap.View?.Settings != null) {
						GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

						GUILayout.BeginHorizontal();

						GUILayout.Label("Supported Modes", BlueStonez.label_interparkbold_11pt_left, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
						GUILayout.Space(ParadiseGUITools.ITEM_SPACING_H);

						GUILayout.BeginVertical(GUILayout.MaxWidth(ParadiseGUITools.INPUT_WIDTH));

						foreach (var mode in Enum.GetValues(typeof(GameModeType)).Cast<GameModeType>().Where(_ => _ != GameModeType.None)) {
							GUILayout.Toggle(uberstrikeMap.View.Settings.ContainsKey(mode), mode.ToString(), GUILayout.Height(22f));
						}

						GUILayout.EndVertical();

						GUILayout.EndHorizontal();
					}
				});
			}
		}
	}
}
