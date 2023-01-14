using HarmonyLib;
using log4net;
using System;
using System.Reflection;

namespace Paradise.Client {
	public class PlayerKilledSpectatorStateHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Moves a player to Spectator state if killed while playing Team Elimination.
		/// </summary>
		public PlayerKilledSpectatorStateHook() { }

		public override void Hook(Harmony harmonyInstance) {
			var type = typeof(ApplicationDataManager).Assembly.GetType("PlayerKilledSpectatorState");

			Log.Info($"[{nameof(PlayerKilledSpectatorStateHook)}] hooking {type.Name}");

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
