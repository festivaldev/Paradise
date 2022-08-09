using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class MenuPageManagerHook : IParadiseHook {
		public void Hook(Harmony harmonyInstance) {
			Debug.Log($"[{typeof(MenuPageManagerHook)}] hooking {typeof(MenuPageManager)}");

			var orig_MenuPageManager_LoadPage = typeof(MenuPageManager).GetMethod("LoadPage", BindingFlags.Instance | BindingFlags.Public);
			var prefix_MenuPageManager_LoadPage = typeof(MenuPageManagerHook).GetMethod("LoadPage_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_MenuPageManager_LoadPage, new HarmonyMethod(prefix_MenuPageManager_LoadPage), null);
		}

		public static bool LoadPage_Prefix(MenuPageManager __instance, PageType pageType, bool forceReload = false) {
			if (pageType == PageType.Home) {
				UnityRuntime.StartRoutine(CustomMapManager.GetCustomMaps());

				UnityRuntime.StartRoutine(GameObject.Find("Plugin Holder").GetComponent<ParadiseUpdater>().CheckForUpdatesIfNecessary(
					OnUpdateAvailableCallback,
					OnUpdateErrorCallback
				));
			}

			return true;
		}

		private static void OnUpdateAvailableCallback(UpdatePlatformDefinition updateDefinition) {
			PopupSystem.ShowMessage("Update available", $"A mandatory update is available ({updateDefinition.version}). You need to install this update in order to play.", PopupSystem.AlertType.OK, delegate {
				UnityRuntime.StartRoutine(GameObject.Find("Plugin Holder").GetComponent<ParadiseUpdater>().InstallUpdates(
					OnUpdateCompleteCallback,
					OnUpdateErrorCallback
				));
			});
		}

		private static void OnUpdateCompleteCallback() {
			PopupSystem.ShowMessage("Update Complete", "Updates have been installed successfully. UberStrike needs to be restarted for completing the installation.", PopupSystem.AlertType.OK, delegate {
				System.Diagnostics.Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "UberStrike.exe")).WaitForExit(1000);
				Application.Quit();
			});
		}

		private static void OnUpdateErrorCallback(string message) {
			Debug.LogError(message);
			PopupSystem.ShowMessage("Error", message, PopupSystem.AlertType.OK);
		}
	}
}
