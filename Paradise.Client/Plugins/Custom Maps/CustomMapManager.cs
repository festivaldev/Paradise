using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	public class CustomMapManager : MonoBehaviour {
		public static CustomMapManager Instance;

		private static AssetBundle bundle;
		private static List<UberstrikeCustomMapView> Maps;
		private static bool IsLoading = false;

		public void Awake() {
			Instance = this;

			var harmony = new Harmony("tf.festival.Paradise.CustomMapManager_hook");
			var orig_MapManager_LoadMap = typeof(MapManager).GetMethod("LoadMap", BindingFlags.Public | BindingFlags.Instance);
			var prefix_MapManager_LoadMap = typeof(CustomMapManager).GetMethod("LoadMap_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmony.Patch(orig_MapManager_LoadMap, new HarmonyMethod(prefix_MapManager_LoadMap), null);
		}

		public static bool LoadMap_Prefix(MapManager __instance, UberstrikeMap map, Action onSuccess) {
			PickupItem.Reset();
			Debug.LogWarning($"Loading custom map: {map.SceneName}");

			if (IsBundleMap(map.Id)) {
				LoadBundle(map.Id, delegate {
					Singleton<SceneLoader>.Instance.LoadLevel(map.SceneName, delegate {
						if (onSuccess != null) {
							onSuccess();
						}
						Debug.LogWarning("Finished Loading custom map");
					});
				});
			} else {
				Singleton<SceneLoader>.Instance.LoadLevel(map.SceneName, delegate {
					if (onSuccess != null) {
						onSuccess();
					}
					Debug.LogWarning("Finished Loading custom map");
				});
			}

			return false;
		}


		public static bool IsBundleMap(int mapid) {
			foreach (var map in Maps) {
				Debug.Log($"isBundleMap: {map.Name} {map.FileName} {map.MapId}");
				if (map.MapId == mapid) {
					return true;
				}
			}

			return false;
		}

		public static void LoadBundle(int mapId, Action onSucces = null) {
			if (!IsLoading) {
				try {
					bundle.Unload(true);
				} catch { }

				UnityRuntime.StartRoutine(LoadBundleMap(mapId, onSucces));
			}
		}

		private static UberstrikeCustomMapView GetMap(int mapId) {
			foreach (UberstrikeCustomMapView map in Maps) {
				if (map.MapId.Equals(mapId)) {
					return map;
				}
			}
			return null;
		}

		public static IEnumerator GetCustomMaps() {
			yield return ParadiseApplicationWebServiceClient.GetCustomMaps("4.7.1", DefinitionType.StandardDefinition, delegate (List<UberstrikeCustomMapView> callback) {
				Maps = callback;
			}, delegate (Exception ex) {
				PopupSystem.ShowMessage("Error", $"There was an error loading the maps.", PopupSystem.AlertType.OK);
			});
		}

		public static IEnumerator LoadBundleMap(int mapId, Action onSuccess = null) {
			IsLoading = true;

			var map = GetMap(mapId);
			if (map == null) {
				PopupSystem.ShowMessage("Bundle Error", "The map you are trying to load cannot be found.");
				yield break;
			}

			var progressPopup = new ProgressPopupDialog("Loading", "Loading Map", null);
			PopupSystem.Show(progressPopup);

			var path = $"file:///{Application.dataPath}/Maps/{map.FileName}";

			using (WWW loader = WWW.LoadFromCacheOrDownload(path, 1)) {
				while (!loader.isDone) {
					progressPopup.Progress = loader.progress;
					yield return null;
				}

				if (string.IsNullOrEmpty(loader.error)) {
					bundle = loader.assetBundle;
					PopupSystem.HideMessage(progressPopup);

					try {
						bundle.LoadAll();
					} catch (Exception e) {
						PopupSystem.ShowMessage("Error", $"An error occured while loading the map: {e.Message}", PopupSystem.AlertType.OK);
					}
				} else {
					PopupSystem.HideMessage(progressPopup);
					PopupSystem.ShowMessage("Error", $"An error occured while loading the map: {loader.error}", PopupSystem.AlertType.OK);
					yield return null;
				}
			}

			if (onSuccess != null) {
				onSuccess();
			}

			IsLoading = false;
		}
	}
}
