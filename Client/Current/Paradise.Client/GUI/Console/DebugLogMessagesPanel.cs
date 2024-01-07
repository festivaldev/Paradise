using HarmonyLib;
using log4net.Core;
using System;
using System.Linq;
using System.Text;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

namespace Paradise.Client {
	[HarmonyPatch]
	public class DebugLogMessagesPanel : IDebugPage {
		public string Title => "Log";

		public static readonly ConsoleDebug Console = new ConsoleDebug();

		private Vector2 consoleScrollPos;
		private bool autoScroll = true;

		public DebugLogMessagesPanel() {
			Console.OnLog += delegate (string log) {
				if (autoScroll) consoleScrollPos.y = float.MaxValue;
			};
		}

		public void Draw() {
			ParadiseGUITools.DrawGroup("Log", delegate {
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Copy Text", BlueStonez.buttondark_small, GUILayout.Width(64f), GUILayout.Height(22f))) {
					TextEditor editor = new TextEditor {
						content = new GUIContent(Console.DebugOut)
					};

					editor.SelectAll();
					editor.Copy();
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_H);

				if (GUILayout.Button("Copy HTML", BlueStonez.buttondark_small, GUILayout.Width(64f), GUILayout.Height(22f))) {
					TextEditor editor = new TextEditor {
						content = new GUIContent(Console.ToHTML())
					};

					editor.SelectAll();
					editor.Copy();
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				autoScroll = GUILayout.Toggle(autoScroll, "Enable Auto Scroll", BlueStonez.toggle);

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				consoleScrollPos = GUILayout.BeginScrollView(consoleScrollPos, false, true, GUIStyle.none, BlueStonez.verticalScrollbar, BlueStonez.scrollView);
				GUILayout.TextArea(Console.DebugOut, BlueStonez.textArea);
				GUILayout.EndScrollView();
			});
		}

		public class ConsoleDebug {
			public event Action<string> OnLog;

			private readonly LimitedQueue<string> _queue = new LimitedQueue<string>(300);
			private string _debugOut = string.Empty;

			public string DebugOut => _debugOut;

			public void Log(Level level, string message, DateTime timestamp) {
				_queue.Enqueue($"[{timestamp:u}] [{level}] {message}");

				var stringBuilder = new StringBuilder();
				foreach (var value in _queue) {
					stringBuilder.AppendLine(value);
				}

				_debugOut = stringBuilder.ToString();

				OnLog?.Invoke(_queue.Last());
			}

			public string ToHTML() {
				var stringBuilder = new StringBuilder();

				stringBuilder.AppendLine(
$@"<!DOCTYPE html>
<html>
  <head>
    <title>Paradise Debug Log</title>
  </head>
  <body>
    <h1>DEBUG LOG</h1>
	<h4>Created at {DateTime.UtcNow:u}</h4>
    <pre>");

				foreach (string str in _queue) {
					stringBuilder.AppendLine(str);
				}

				stringBuilder.AppendLine(
@"    </pre>
  </body>
</html>");
				return stringBuilder.ToString();
			}
		}

		[HarmonyPatch(typeof(Debug), "Log", typeof(object)), HarmonyPrefix]
		public static bool Debug_Log_Prefix(object message) {
			Console.Log(Level.Info, message?.ToString() ?? "Null", DateTime.UtcNow);
			return true;
		}

		[HarmonyPatch(typeof(Debug), "LogError", typeof(object)), HarmonyPrefix]
		public static bool Debug_LogError_Prefix(object message) {
			Console.Log(Level.Error, message?.ToString() ?? "Null", DateTime.UtcNow);
			return true;
		}

		[HarmonyPatch(typeof(Debug), "LogException", typeof(Exception)), HarmonyPrefix]
		public static bool Debug_LogException_Prefix(Exception exception) {
			Console.Log(Level.Fatal, exception.ToString(), DateTime.UtcNow);
			return true;
		}

		[HarmonyPatch(typeof(Debug), "LogWarning", typeof(object)), HarmonyPrefix]
		public static bool Debug_LogWarning_Prefix(object message) {
			Console.Log(Level.Warn, message?.ToString() ?? "Null", DateTime.UtcNow);
			return true;
		}
	}
}
