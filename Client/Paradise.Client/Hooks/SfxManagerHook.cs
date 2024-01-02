using HarmonyLib;
using log4net;
using System;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Restores "missing" announcer lines by converting the sound delay from seconds to milliseconds. (Why, oh why, Cmune??)
	/// </summary>
	[HarmonyPatch(typeof(SfxManager))]
	public class SfxManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(SfxManagerHook));

		static SfxManagerHook() {
			Log.Info($"[{nameof(SfxManagerHook)}] hooking {nameof(SfxManager)}");
		}

		[HarmonyPatch("Play2dAudioClip", new Type[] { typeof(AudioClip), typeof(ulong), typeof(float), typeof(float) }), HarmonyPostfix]
		public static void Play2dAudioClip_Postfix(AudioClip audioClip, ulong delay, float volume, float pitch) {
			if (audioClip == null) {
				return;
			}

			AudioSource uiAudioSource = (AudioSource)typeof(SfxManager).GetField("uiAudioSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AutoMonoBehaviour<SfxManager>.Instance);

			if (delay > 0UL) {
				uiAudioSource.clip = audioClip;
				uiAudioSource.PlayDelayed(delay / 1000f);
			} else {
				uiAudioSource.PlayOneShot(audioClip);
			}

			ApplicationOptions applicationOptions = ApplicationDataManager.ApplicationOptions;
			float volumeMultiplier = (!applicationOptions.AudioEnabled) ? 0f : (applicationOptions.AudioEffectsVolume * applicationOptions.AudioMasterVolume);

			uiAudioSource.volume = volumeMultiplier * volume;
			uiAudioSource.pitch = pitch;
		}
	}
}
