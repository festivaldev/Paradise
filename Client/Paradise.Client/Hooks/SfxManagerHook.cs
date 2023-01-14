using HarmonyLib;
using log4net;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class SfxManagerHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Restores "missing" announcer lines by converting the sound delay from seconds to milliseconds. (Why, oh why, Cmune??)
		/// </summary>
		public SfxManagerHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(SfxManagerHook)}] hooking {nameof(SfxManager)}");

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
