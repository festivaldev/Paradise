using HarmonyLib;
using log4net;
using System.Collections;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Reduces the wait time for connecting to the lobby chat after the main menu has loaded
	/// </summary>
	[HarmonyPatch(typeof(CommConnectionManager))]
	public class CommConnectionManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(CommConnectionManagerHook));

		static CommConnectionManagerHook() {
			Log.Info($"[{nameof(CommConnectionManagerHook)}] hooking {typeof(CommConnectionManager)}");
		}

		[HarmonyPatch("StartCheckingCommServerConnection"), HarmonyPrefix]
		public static bool StartCheckingCommServerConnection_Prefix() {
			return false;
		}

#pragma warning disable CS0162
		[HarmonyPatch("StartCheckingCommServerConnection"), HarmonyPostfix]
		public static IEnumerator StartCheckingCommServerConnection_Postfix(IEnumerator value, CommConnectionManager __instance) {
			var client = new Traverse(__instance).Property<CommPeer>("Client").Value;

			for (;;) {
				if (client.IsEnabled && !client.IsConnected && Singleton<GameServerManager>.Instance.CommServer.IsValid && PlayerDataManager.IsPlayerLoggedIn) {
					client.Connect(Singleton<GameServerManager>.Instance.CommServer.ConnectionString);
				}

				yield return new WaitForSeconds(5f);
			}

			yield break;
		}
	}
#pragma warning restore CS0162
}
