using HarmonyLib;
using log4net;
using System;

namespace Paradise.Client {
	/// <summary>
	/// Allows for loading custom local maps.
	/// </summary>
	[HarmonyPatch(typeof(MapManager))]
	public class MapManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(MapManagerHook));

		static MapManagerHook() {
			Log.Info($"[{nameof(MapManagerHook)}] hooking {nameof(MapManager)}");
		}

		[HarmonyPatch("LoadMap"), HarmonyPrefix]
		public static bool LoadMap_Prefix(MapManager __instance, UberstrikeMap map, Action onSuccess) {
			PickupItem.Reset();

			Log.Debug($"Loading map from scene {map.SceneName}");

			if (CustomMapManager.IsBundleMap(map.Id)) {
				CustomMapManager.LoadBundle(map.Id, delegate {
					Singleton<SceneLoader>.Instance.LoadLevel(map.SceneName, delegate {
						Log.Info($"Finished loading custom map \"{map.Name}\"");

						onSuccess?.Invoke();
					});
				});
			} else {
				Singleton<SceneLoader>.Instance.LoadLevel(map.SceneName, delegate {
					Log.Info($"Finished loading default map \"{map.Name}\"");

					onSuccess?.Invoke();
				});
			}

			return false;
		}
	}
}
