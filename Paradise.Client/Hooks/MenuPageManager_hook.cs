using HarmonyLib;
using System.Reflection;

namespace Paradise.Client {
	public static class MenuPageManager_hook {
		private static bool DidCheckForUpdates;

		public static void Hook() {
			var harmony = new Harmony("tf.festival.Paradise.GlobalSceneLoader_hook");

			var orig_MenuPageManager_LoadPage = typeof(MenuPageManager).GetMethod("LoadPage", BindingFlags.Instance | BindingFlags.Public);
			var prefix_MenuPageManager_LoadPage = typeof(MenuPageManager_hook).GetMethod("LoadPage_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmony.Patch(orig_MenuPageManager_LoadPage, new HarmonyMethod(prefix_MenuPageManager_LoadPage), null);
		}

		private static void OnUpdateAvailableCallback(UpdatePlatformDefinition updateDefinition) {
			PopupSystem.ShowMessage("Update available", $"A mandatory update is available ({updateDefinition.version}). You need to install this update in order to play.", PopupSystem.AlertType.OK, delegate {
				UnityRuntime.StartRoutine(ParadiseUpdater.InstallUpdates());
			});
		}

		public static bool LoadPage_Prefix(MenuPageManager __instance, PageType pageType, bool forceReload = false) {
			if (pageType == PageType.Home && !DidCheckForUpdates) {
				DidCheckForUpdates = true;

				UnityRuntime.StartRoutine(ParadiseUpdater.CheckForUpdates(OnUpdateAvailableCallback, null));
			}

			return true;
		}
	}
}
