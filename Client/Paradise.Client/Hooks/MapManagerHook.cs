using HarmonyLib;
using log4net;
using System;
using System.Reflection;

namespace Paradise.Client {
	public class MapManagerHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Allows for loading custom local maps.
		/// </summary>
		public MapManagerHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(MapManagerHook)}] hooking {nameof(MapManager)}");

			var orig_MapManager_LoadMap = typeof(MapManager).GetMethod("LoadMap", BindingFlags.Public | BindingFlags.Instance);
			var prefix_MapManager_LoadMap = typeof(MapManagerHook).GetMethod("LoadMap_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_MapManager_LoadMap, new HarmonyMethod(prefix_MapManager_LoadMap), null);
		}

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
