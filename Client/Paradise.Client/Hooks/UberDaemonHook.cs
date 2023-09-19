using HarmonyLib;
using log4net;
using System.Diagnostics;
using UnityEngine;

namespace Paradise.Client {
	[HarmonyPatch(typeof(UberDaemon))]
	public class UberDaemonHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(UberDaemonHook));

		static UberDaemonHook() {
			Log.Info($"[{nameof(UberDaemonHook)}] hooking {nameof(UberDaemon)}");
		}

		[HarmonyPatch("GetMagicHash"), HarmonyPrefix]
		public static bool UberDaemon_GetMagicHash_Prefix() {
			return false;
		}

		[HarmonyPatch("GetMagicHash"), HarmonyPostfix]
		public static void UberDaemon_GetMagicHash_Postfix(string authToken, ref string __result) {
			var processStartInfo = new ProcessStartInfo {
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Minimized,
				CreateNoWindow = true
			};

			if (Application.platform == RuntimePlatform.WindowsPlayer) {
				processStartInfo.FileName = "uberdaemon_paradise.exe";
				processStartInfo.Arguments = authToken;
			} else {
				processStartInfo.FileName = "/usr/bin/bash";
				processStartInfo.Arguments = $"uberdaemon_paradise.sh {authToken}";
			}

			var process = Process.Start(processStartInfo);

			__result = process.StandardOutput.ReadToEnd().Trim();
		}
	}
}
