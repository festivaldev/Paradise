using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	internal class CustomMapManager {
		private static readonly ILog Log = LogManager.GetLogger(nameof(CustomMapManager));

		public static List<ParadiseMapView> Maps { get; private set; } = new List<ParadiseMapView>();

		private static AssetBundle Bundle;
		private static bool IsLoading = false;

		public static IEnumerator GetCustomMaps() {
			Log.Info("Getting custom maps from server");
			yield return ParadiseApplicationWebServiceClient.GetCustomMaps(ApplicationDataManager.Version, DefinitionType.StandardDefinition, delegate (List<ParadiseMapView> callback) {
				Maps = callback;
			}, delegate (Exception e) {
				PopupSystem.ShowMessage("Error", $"There was an error loading the maps.", PopupSystem.AlertType.OK);
				Log.Info($"There was an error loading the maps\n{e.Message}");
				Log.Debug(e);
			});
		}

		private static ParadiseMapView GetMap(int mapId) {
			foreach (ParadiseMapView map in Maps) {
				if (map.MapId.Equals(mapId)) {
					return map;
				}
			}
			return null;
		}

		public static bool IsBundleMap(int mapid) {
			foreach (var map in Maps) {
				if (map.MapId == mapid) {
					return true;
				}
			}

			return false;
		}

		public static void LoadBundle(int mapId, Action onSucces = null) {
			if (!IsLoading) {
				try {
					Bundle.Unload(true);
				} catch { }

				UnityRuntime.StartRoutine(LoadBundleMap(mapId, onSucces));
			}
		}

		public static IEnumerator LoadBundleMap(int mapId, Action successCallback = null) {
			IsLoading = true;

			var map = GetMap(mapId);

			if (map == null) {
				PopupSystem.ShowMessage("Bundle Error", "The map you are trying to load cannot be found.");

				yield break;
			}

			var progressPopup = new ProgressPopupDialog("Loading", $"Loading map from {map.FileName}", null);
			PopupSystem.Show(progressPopup);

			var path = $"file:///{Application.dataPath}/Maps/{map.FileName}";

			using (WWW loader = WWW.LoadFromCacheOrDownload(path, 1)) {
				while (!loader.isDone) {
					progressPopup.Progress = loader.progress;

					yield return null;
				}

				if (string.IsNullOrEmpty(loader.error)) {
					Bundle = loader.assetBundle;
					PopupSystem.HideMessage(progressPopup);

					try {
						Bundle.LoadAll();
					} catch (Exception e) {
						PopupSystem.ShowMessage("Error", $"An error occured while loading the map: {e.Message}", PopupSystem.AlertType.OK);
					}
				} else {
					PopupSystem.HideMessage(progressPopup);
					PopupSystem.ShowMessage("Error", $"An error occured while loading the map: {loader.error}", PopupSystem.AlertType.OK);

					yield return null;
				}
			}

			successCallback?.Invoke();

			IsLoading = false;
		}
	}
}
