using HarmonyLib;
using log4net;
using System;
using System.Collections;
using Cmune.DataCenter.Common.Entities;
using UberStrike.DataCenter.Common.Entities;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Reduces the wait time for connecting to the lobby chat after the main menu has loaded
	/// </summary>
	[HarmonyPatch(typeof(CommConnectionManager))]
	public class CommConnectionManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(CommConnectionManagerHook));

		private static ParadiseTraverse<CommConnectionManager> traverse;

		static CommConnectionManagerHook() {
			Log.Info($"[{nameof(CommConnectionManagerHook)}] hooking {typeof(CommConnectionManager)}");
		}

		[HarmonyPatch("Awake"), HarmonyPrefix]
		public static bool Awake_Prefix(CommConnectionManager __instance) {
			if (traverse == null) {
				traverse = ParadiseTraverse<CommConnectionManager>.Create(__instance);
			}

			traverse.SetProperty("Client", new CommPeer());
			EventHandler.Global.AddListener(delegate(GlobalEvents.Login ev) { traverse.InvokeMethod("OnLoginEvent", ev); });

			return false;
		}

		public static void Connect(CommConnectionManager __instance) {
			traverse.Instance.Client?.Disconnect();

			traverse.SetProperty("Client", new CommPeer());
			traverse.Instance.StartCoroutine(traverse.InvokeMethod<IEnumerator>("StartCheckingCommServerConnection"));
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
#pragma warning restore CS0162

		[HarmonyPatch(typeof(CompleteAccountPanelGUI), "CompleteAccountCallback"), HarmonyPrefix]
		public static bool CompleteAccountPanelGUI_CompleteAccountCallback_Prefix(CompleteAccountPanelGUI __instance, AccountCompletionResultView result, string name) {
			if (result.Result == AccountCompletionResult.Ok) {
				Connect(AutoMonoBehaviour<CommConnectionManager>.Instance);
			}

			return true;
		}
	}
}
