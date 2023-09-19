using System.Text;
using UberStrike.WebService.Unity;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugWebServicesPanel : IDebugPage {
		public string Title => "Web Services";

		private readonly StringBuilder requestLog = new StringBuilder();
		private string currentLog = string.Empty;

		private Vector2 consoleScrollPos;
		private bool autoScroll = true;

		public DebugWebServicesPanel() {
			Configuration.RequestLogger = delegate (string log) {
				requestLog.AppendLine(log);
				currentLog = requestLog.ToString();

				if (autoScroll) consoleScrollPos.y = float.MaxValue;
			};
		}

		public void Draw() {
			ParadiseGUITools.DrawGroup("Web Services", delegate {
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Inbound Traffic", ParadiseGUITools.FormatSize(WebServiceStatistics.TotalBytesIn));
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Outbound Traffic", ParadiseGUITools.FormatSize(WebServiceStatistics.TotalBytesOut));

				foreach (var keyValuePair in WebServiceStatistics.Data) {
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					var statistic = keyValuePair.Value;
					ParadiseGUITools.DrawTextField(keyValuePair.Key, $"count: {statistic.Counter}/{statistic.FailCounter} | time: {statistic.Time:F2} | data: {ParadiseGUITools.FormatSize(statistic.IncomingBytes)}/{ParadiseGUITools.FormatSize(statistic.OutgoingBytes)}");
				}
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Log", delegate {
				autoScroll = GUILayout.Toggle(autoScroll, "Enable Auto Scroll", BlueStonez.toggle);

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				consoleScrollPos = GUILayout.BeginScrollView(consoleScrollPos, false, true, GUIStyle.none, BlueStonez.verticalScrollbar, BlueStonez.scrollView, GUILayout.Height(300f));
				GUILayout.TextArea(currentLog, BlueStonez.textArea);
				GUILayout.EndScrollView();
			});
		}
	}
}
