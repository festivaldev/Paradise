using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Paradise.Client {
	public class SfxManagerHook : IParadiseHook {
		public void Hook(Harmony harmonyInstance) {
			Debug.Log($"[{typeof(SfxManagerHook)}] hooking {typeof(SfxManager)}");

			var orig_SfxManager_Play2dAudioClip = typeof(SfxManager).GetMethods().Where((p) =>
				p.Name == "Play2dAudioClip" &&
				p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(AudioClip), typeof(ulong), typeof(float), typeof(float) }) &&
				p.ReturnType == typeof(void)
			).First();
			var postfix_SfxManager_Play2dAudioClip = typeof(SfxManagerHook).GetMethod("Play2dAudioClip_postfix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_SfxManager_Play2dAudioClip, null, new HarmonyMethod(postfix_SfxManager_Play2dAudioClip), null);
		}

		public static void Play2dAudioClip_postfix(AudioClip audioClip, ulong delay, float volume, float pitch) {
			if (audioClip == null) {
				return;
			}

			AudioSource uiAudioSource = (AudioSource)typeof(SfxManager).GetField("uiAudioSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AutoMonoBehaviour<SfxManager>.Instance);

			if (delay > 0UL) {
				uiAudioSource.clip = audioClip;
				UnityEngine.Debug.Log($"audioClip: {audioClip}, delay: {delay} ({delay / 1000f})");
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
