using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UberStrike.Core.Types;

namespace Paradise.Client {
	public class PlayerKilledSpectatorStateHook : IParadiseHook {
		public Type TypeToHook => typeof(ApplicationDataManager).Assembly.GetType("PlayerKilledSpectatorState");

		public void Hook() {
			var harmony = new Harmony("tf.festival.Paradise.PlayerKilledSpectatorStateHook");

			var orig_PlayerKilledSpectatorState_OnEnter = TypeToHook.GetMethod("OnEnter", BindingFlags.Instance | BindingFlags.Public);
			var postfix_PlayerKilledSpectatorState_OnEnter = typeof(PlayerKilledSpectatorStateHook).GetMethod("OnEnter_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmony.Patch(orig_PlayerKilledSpectatorState_OnEnter, null, new HarmonyMethod(postfix_PlayerKilledSpectatorState_OnEnter));
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
