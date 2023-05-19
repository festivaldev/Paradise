using HarmonyLib;
using log4net;
using System;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Allows staff members to toggle the Free Camera inside Training Mode (aka "Explore Maps").
	/// </summary>
	[HarmonyPatch(typeof(TrainingRoom))]
	public class TrainingRoomHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(TrainingRoomHook));

		private static bool IsFreeCamera;

		static TrainingRoomHook() {
			Log.Info($"[{nameof(TrainingRoomHook)}] hooking {nameof(TrainingRoom)}");
		}

		[HarmonyPatch(MethodType.Constructor), HarmonyPrefix]
		public static bool TrainingRoom_ctor_Prefix(TrainingRoom __instance) {
			var spectatingState = typeof(TrainingRoom).Assembly.GetType("PlayerSpectatingState");

			GameState.Current.PlayerState.RegisterState(PlayerStateId.Spectating, (IState)Activator.CreateInstance(spectatingState, new[] { GameState.Current.PlayerState }));

			return true;
		}

		[HarmonyPatch("OnUpdate"), HarmonyPostfix]
		public static void TrainingRoom_OnUpdate_Postfix() {
			if (KeyInput.AltPressed && KeyInput.CtrlPressed && KeyInput.GetKeyDown(KeyCode.C)) {
				if (!IsFreeCamera) {
					IsFreeCamera = true;

					GameState.Current.PlayerState.SetState(PlayerStateId.Spectating);
					GameObject.Find("HUDNotifications").GetComponent<HUDNotifications>().Hide(true);
				} else {
					IsFreeCamera = false;
					GameState.Current.PlayerState.SetState(PlayerStateId.Playing);
				}
			}
		}

		[HarmonyPatch(typeof(AvatarDecorator), "PlayFootSound", new Type[] { typeof(float), typeof(FootStepSoundType) }), HarmonyPrefix]
		public static bool CharacterMoveController_UpdatePlayerMovement_Prefix() {
			if (GameState.Current.GameMode != UberStrike.Core.Types.GameModeType.None) return true;

			return !IsFreeCamera;
		}

		[HarmonyPatch(typeof(LocalPlayer), "OnCharacterGrounded"), HarmonyPrefix]
		public static bool LocalPlayer_OnCharacterGrounded_Prefix() {
			if (GameState.Current.GameMode != UberStrike.Core.Types.GameModeType.None) return true;

			return !IsFreeCamera;
		}

		[HarmonyPatch(typeof(HUDNotifications), "ShowCrt"), HarmonyPrefix]
		public static bool HUDNotifications_ShowCrt_Prefix() {
			if (GameState.Current.GameMode != UberStrike.Core.Types.GameModeType.None) return true;

			return !IsFreeCamera;
		}
	}
}
