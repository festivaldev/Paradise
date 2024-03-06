using HarmonyLib;
using log4net;
using UberStrike.Core.Models;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Restores "missing" announcer lines by converting the sound delay from seconds to milliseconds. (Why, oh why, Cmune??)
	/// </summary>
	[HarmonyPatch(typeof(LocalPlayer))]
	public class LocalPlayerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(LocalPlayerHook));

		private static readonly ParadiseTraverse<LocalPlayer> traverse;

		static LocalPlayerHook() {
			Log.Info($"[{nameof(LocalPlayerHook)}] hooking {nameof(LocalPlayer)}");

			traverse = ParadiseTraverse<LocalPlayer>.Create();
		}

		[HarmonyPatch("OnCharacterGrounded"), HarmonyPrefix]
		public static bool OnCharacterGrounded_Prefix(LocalPlayer __instance, float velocity) {
			if (traverse.Instance == null) {
				traverse.Instance = __instance;
			}

			if (/*GameState.Current.HasJoinedGame && GameState.Current.IsInGame &&*/ !WeaponFeedbackManager.IsBobbing && traverse.GetField<float>("_lastGrounded") + 0.5f < Time.time && !GameState.Current.PlayerData.Is(MoveStates.Diving)) {
				traverse.SetField("_lastGrounded", Time.time);

				if (__instance.Character != null && __instance.Character.Avatar != null && __instance.Character.Avatar.Decorator != null) {
					__instance.Character.Avatar.Decorator.PlayFootSound(__instance.Character.WalkingSoundSpeed);
					if (velocity < -20f) {
						LevelCamera.DoLandFeedback(true);

						if (ParadiseClient.Settings.EnableLandingGrunt) {
							AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.LandingGrunt);
						}
					} else {
						LevelCamera.DoLandFeedback(false);
					}
				}
			}

			return false;
		}
	}
}
