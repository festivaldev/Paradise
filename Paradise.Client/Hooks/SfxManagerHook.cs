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
			var prefix_SfxManager_Play2dAudioClip = typeof(SfxManagerHook).GetMethod("Play2dAudioClip_prefix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_SfxManager_Play2dAudioClip, new HarmonyMethod(prefix_SfxManager_Play2dAudioClip), null);
		}

		public static void Play2dAudioClip_prefix(AudioClip audioClip, ref ulong delay, float volume, ref float pitch) {
			if (delay > 0UL) { delay = 0UL; }
		} 
	}
}
