using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugAudioPanel : IDebugPage {
		public string Title => "Audio";

		private readonly AudioSource audioSource = GameObject.Find("Plugin Holder").AddComponent<AudioSource>();
		private readonly List<PropertyInfo> audioClips;

		public DebugAudioPanel() {
			audioSource.loop = false;
			audioSource.playOnAwake = false;
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.priority = 100;

			audioClips = AccessTools.GetDeclaredProperties(typeof(GameAudio))
				.Where(_ => _.PropertyType.Equals(typeof(AudioClip)))
				.OrderBy(_ => _.Name)
				.ToList();
		}

		public void Draw() {
			ParadiseGUITools.DrawGroup("Audio Controls", delegate {
				// Volume
				var volume = audioSource.volume;
				ParadiseGUITools.DrawSlider("Volume", ref volume, 0f, 1f, delegate (float value) {
					GUILayout.Label($"{value * 100:F0} %", BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (volume != audioSource.volume) {
					audioSource.volume = volume;
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				// Pitch
				var pitch = audioSource.pitch;
				ParadiseGUITools.DrawSlider("Pitch", ref pitch, 0f, 4f, delegate (float value) {
					GUILayout.Label($"{value:F2}", BlueStonez.label_interparkbold_11pt_left, GUILayout.Width(ParadiseGUITools.SLIDER_VALUE_WIDTH), GUILayout.Height(22f));
				});

				if (pitch != audioSource.pitch) {
					audioSource.pitch = (float)Math.Round(pitch, 2);
				}

				GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				// Audio Controls
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Stop sound", BlueStonez.buttondark_medium, GUILayout.Height(22f))) {
					audioSource.Stop();
					audioSource.clip = null;
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				if (GUILayout.Button("Stop background sound", BlueStonez.buttondark_medium, GUILayout.Height(22f))) {
					AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
				}

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				if (GUILayout.Button("Stop ALL sound", BlueStonez.buttondark_medium, GUILayout.Height(22f))) {
					audioSource.Stop();
					audioSource.clip = null;

					AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
				}

				GUILayout.EndHorizontal();
			});

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup("Audio Clips", delegate {
				var buttonWidth = ParadiseGUITools.GetButtonWidth(audioClips.Select(_ => _.Name), BlueStonez.buttondark_medium);

				//foreach (var audioClip in audioClips) {
				foreach (var item in audioClips.ToList().Select((x, i) => new { Value = x, Index = i })) {
					if (item.Index > 0) GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					var clip = (AudioClip)item.Value.GetValue(AutoMonoBehaviour<SfxManager>.Instance, null);

					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();

					if (clip != null) {
						if (GUILayout.Button(item.Value.Name, BlueStonez.buttondark_medium, GUILayout.Width(buttonWidth), GUILayout.Height(22f))) {
							audioSource.Stop();
							audioSource.clip = clip;
							audioSource.Play();
						}
					} else {
						GUITools.PushGUIState();
						GUI.enabled = false;

						GUILayout.Button($"{item.Value.Name} (not loaded)", BlueStonez.buttondark_medium, GUILayout.Width(buttonWidth), GUILayout.Height(22f));

						GUI.enabled = true;
						GUITools.PopGUIState();
					}

					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
			});
		}
	}
}
