using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugAudio : IDebugPage {
		public string Title => "Audio";

		private AudioSource audio;
		private Vector2 scroll;

		private IEnumerable<PropertyInfo> AudioClipProperties;

		public DebugAudio(AudioSource audio) {
			this.audio = audio;

			AudioClipProperties = typeof(GameAudio)
				.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.ToList()
				.Where(_ => _.PropertyType.Equals(typeof(AudioClip)))
				.OrderBy(_ => _.Name);
		}

		public void Draw() {
			GUILayout.Label("AudioListener.volume " + AudioListener.volume, new GUILayoutOption[0]);
			GUILayout.Label("AudioListener.pause " + AudioListener.pause, new GUILayoutOption[0]);

			if (GUILayout.Button("audio.bypassEffects " + this.audio.bypassEffects, new GUILayoutOption[0])) {
				this.audio.bypassEffects = !this.audio.bypassEffects;
			}

			this.scroll = GUILayout.BeginScrollView(this.scroll, new GUILayoutOption[0]);

			foreach (var audioClipProperty in AudioClipProperties) {
				var clip = (AudioClip)audioClipProperty.GetValue(AutoMonoBehaviour<SfxManager>.Instance, null);

				if (clip != null) {
					if (GUILayout.Button(audioClipProperty.Name, new GUILayoutOption[0])) {
						AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(clip, 0UL, 1f, 1f);
					}
				} else {
					GUILayout.Label(audioClipProperty.Name + " not loaded", new GUILayoutOption[0]);
				}
			}

			GUILayout.EndScrollView();
		}
	}
}
