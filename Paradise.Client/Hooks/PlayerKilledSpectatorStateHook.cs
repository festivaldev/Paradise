using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class PlayerKilledSpectatorStateHook : IParadiseHook {
		public void Hook(Harmony harmonyInstance) {
			var type = typeof(ApplicationDataManager).Assembly.GetType("PlayerKilledSpectatorState");

			Debug.Log($"[{typeof(PlayerKilledSpectatorStateHook)}] hooking {type}");

			var orig_PlayerKilledSpectatorState_OnEnter = type.GetMethod("OnEnter", BindingFlags.Instance | BindingFlags.Public);
			var postfix_PlayerKilledSpectatorState_OnEnter = typeof(PlayerKilledSpectatorStateHook).GetMethod("OnEnter_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_PlayerKilledSpectatorState_OnEnter, null, new HarmonyMethod(postfix_PlayerKilledSpectatorState_OnEnter));
		}

		public static void OnEnter_Postfix() {
			System.Threading.Timer timer = null;
			timer = new System.Threading.Timer(s => {
				if (GameState.Current.MatchState.CurrentStateId == GameStateId.MatchRunning) {
					GameState.Current.PlayerState.SetState(PlayerStateId.Spectating);
				}

				timer.Dispose();
			}, null, 3000, UInt32.MaxValue - 10);
		}
	}
}
