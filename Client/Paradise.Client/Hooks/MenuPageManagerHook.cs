using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class MenuPageManagerHook : IParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		private static bool HasCleanedUpdates;
		private static bool HasRequestedMaps;

		/// <summary>
		/// <br>• Adds game update logic (checked on game startup and every 60 minutes when entering the main menu) and displays a warning if updates are disabled.</br>
		/// <br>• Responsible for requesting custom maps from server once on game startup.</br>
		/// </summary>
		public MenuPageManagerHook() { }

		public void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(MenuPageManagerHook)}] hooking {nameof(MenuPageManager)}");

			var orig_MenuPageManager_LoadPage = typeof(MenuPageManager).GetMethod("LoadPage", BindingFlags.Public | BindingFlags.Instance);
			var prefix_MenuPageManager_LoadPage = typeof(MenuPageManagerHook).GetMethod("LoadPage_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_MenuPageManager_LoadPage, new HarmonyMethod(prefix_MenuPageManager_LoadPage), null);
		}

		public static bool LoadPage_Prefix(MenuPageManager __instance, PageType pageType, bool forceReload = false) {
			if (pageType == PageType.Home) {
				if (!HasCleanedUpdates) {
					HasCleanedUpdates = true;
					UnityRuntime.StartRoutine(ParadiseUpdater.CleanupUpdates());
				}

				if (!HasRequestedMaps) {
					HasRequestedMaps = true;
					UnityRuntime.StartRoutine(CustomMapManager.GetCustomMaps());
				}

				UnityRuntime.StartRoutine(GameObject.Find("Plugin Holder").GetComponent<ParadiseUpdater>().CheckForUpdatesIfNecessary(
					OnUpdateAvailableCallback,
					OnUpdateErrorCallback
				));
			}

			return true;
		}

		private static void OnUpdateAvailableCallback(UpdatePlatformDefinition updateDefinition) {
			PopupSystem.ShowMessage("Update available", $"A mandatory update is available ({updateDefinition.version ?? "Unknown"}, Build {updateDefinition.build ?? "Unknown"}). You need to install this update in order to play.", PopupSystem.AlertType.OKCancel, delegate {
				UnityRuntime.StartRoutine(GameObject.Find("Plugin Holder").GetComponent<ParadiseUpdater>().InstallUpdates(
					OnUpdateCompleteCallback,
					OnUpdateErrorCallback
				));
			}, "Update", delegate {
				if (PlayerDataManager.AccessLevel < MemberAccessLevel.SeniorQA) {
					Application.Quit();
				}
			}, PlayerDataManager.AccessLevel < MemberAccessLevel.SeniorQA ? "Quit" : "Ignore");
		}

		private static void OnUpdateCompleteCallback() {
			PopupSystem.ShowMessage("Update Complete", "Updates have been installed successfully. In order to complete the installation, UberStrike needs to be restarted.", PopupSystem.AlertType.OK, delegate {
				System.Diagnostics.Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "UberStrike.exe")).WaitForExit(1000);
				Application.Quit();
			});
		}

		private static void OnUpdateErrorCallback(string message) {
			Log.Error(message);
			PopupSystem.ShowMessage("Error", message, PopupSystem.AlertType.OK);
		}
	}
}
