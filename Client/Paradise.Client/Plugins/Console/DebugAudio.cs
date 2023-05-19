using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugAudio : IDebugPage {
		public string Title => "Audio";

		private AudioSource AudioSource = GameObject.Find("Plugin Holder").AddComponent<AudioSource>();
		private List<PropertyInfo> AudioClipProperties;

		private Vector2 ScrollPos;

		public DebugAudio() {
			AudioSource.loop = false;
			AudioSource.playOnAwake = false;
			AudioSource.rolloffMode = AudioRolloffMode.Linear;
			AudioSource.priority = 100;

			AudioClipProperties = AccessTools.GetDeclaredProperties(typeof(GameAudio))
				.Where(_ => _.PropertyType.Equals(typeof(AudioClip)))
				.OrderBy(_ => _.Name)
				.ToList();
		}

		public void Draw() {
			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical("box", GUILayout.Width(Math.Min(600f, Screen.width / 3)));
			GUILayout.Label("AUDIO CONTROLS");

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Stop all sound", GUILayout.Width(250))) {
				AudioSource.Stop();
				AudioSource.clip = null;
			}

			if (GUILayout.Button("Stop background sound", GUILayout.Width(250))) {
				AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();


			GUILayout.Space(8);

			AudioSource.volume = HorizontalScrollbar("Volume: ", AudioSource.volume, 0, 1);
			AudioSource.pitch = HorizontalScrollbar("Pitch: ", AudioSource.pitch, 0, 4);

			GUILayout.EndVertical();

			GUILayout.BeginVertical("box", GUILayout.Width(Math.Min(600f, Screen.width / 3)));
			GUILayout.Label("AUDIO Clips");

			ScrollPos = GUILayout.BeginScrollView(ScrollPos);

			foreach (var audioClipProperty in AudioClipProperties) {
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				var clip = (AudioClip)audioClipProperty.GetValue(AutoMonoBehaviour<SfxManager>.Instance, null);

				if (clip != null) {
					if (GUILayout.Button(audioClipProperty.Name, GUILayout.Width(250))) {
						AudioSource.Stop();
						AudioSource.clip = clip;
						AudioSource.Play();
					}
				} else {
					GUI.enabled = false;
					GUILayout.Button($"{audioClipProperty.Name} (not loaded)", GUILayout.Width(250));
					GUI.enabled = true;
				}

				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		private float HorizontalScrollbar(string title, float value, float min, float max) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(title);
			GUILayout.FlexibleSpace();
			value = GUILayout.HorizontalScrollbar(value, 1f, min, (max + 1), GUILayout.Width(300));
			GUILayout.Space(10f);
			GUILayout.Label($"{value:0.00}");
			GUILayout.EndHorizontal();

			return value;
		}
	}
}
