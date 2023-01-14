using HarmonyLib;
using log4net;
using System;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class TrainingRoomHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		private static bool IsFreeCamera;

		/// <summary>
		/// Allows staff members to toggle the Free Camera inside Training Mode (aka "Explore Maps").
		/// </summary>
		public TrainingRoomHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(TrainingRoomHook)}] hooking {nameof(TrainingRoom)}");

			var orig_TrainingRoom_ctor = typeof(TrainingRoom).GetConstructors()[0];
			var prefix_TrainingRoom_ctor = typeof(TrainingRoomHook).GetMethod("ctor_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_TrainingRoom_ctor, new HarmonyMethod(prefix_TrainingRoom_ctor), null);

			var orig_TrainingRoom_OnUpdate = typeof(TrainingRoom).GetMethod("OnUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
			var postfix_TrainingRoom_OnUpdate = typeof(TrainingRoomHook).GetMethod("OnUpdate_Postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_TrainingRoom_OnUpdate, null, new HarmonyMethod(postfix_TrainingRoom_OnUpdate));
		}

		public static bool ctor_Prefix(TrainingRoom __instance) {
			var spectatingState = typeof(TrainingRoom).Assembly.GetType("PlayerSpectatingState");

			GameState.Current.PlayerState.RegisterState(PlayerStateId.Spectating, (IState)Activator.CreateInstance(spectatingState, new[] { GameState.Current.PlayerState }));

			return true;
		}

		public static void OnUpdate_Postfix() {
			if (KeyInput.AltPressed && KeyInput.CtrlPressed && KeyInput.GetKeyDown(KeyCode.C)) {
				if (PlayerDataManager.AccessLevel > Cmune.DataCenter.Common.Entities.MemberAccessLevel.Default) {
					if (!IsFreeCamera) {
						IsFreeCamera = true;
						GameState.Current.PlayerState.SetState(PlayerStateId.Spectating);
					} else {
						IsFreeCamera = false;
						GameState.Current.PlayerState.SetState(PlayerStateId.Playing);
					}
				}
			}
		}
	}
}
