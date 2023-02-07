using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UberStrike.Core.Models;
using UnityEngine;

namespace Paradise.Client {
	public class MenuPageManagerHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		private static bool HasCleanedUpdates;
		private static bool HasRequestedMaps;
		private static bool HasHandledCmdLineArgs;

		/// <summary>
		/// <br>• Adds game update logic (checked on game startup and every 60 minutes when entering the main menu) and displays a warning if updates are disabled.</br>
		/// <br>• Responsible for requesting custom maps from server once on game startup.</br>
		/// </summary>
		public MenuPageManagerHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(MenuPageManagerHook)}] hooking {nameof(MenuPageManager)}");

			var orig_MenuPageManager_LoadPage = typeof(MenuPageManager).GetMethod("LoadPage", BindingFlags.Public | BindingFlags.Instance);
			var prefix_MenuPageManager_LoadPage = typeof(MenuPageManagerHook).GetMethod("LoadPage_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_MenuPageManager_LoadPage, new HarmonyMethod(prefix_MenuPageManager_LoadPage), null);
		}

		public static bool LoadPage_Prefix(MenuPageManager __instance, PageType pageType, bool forceReload = false) {
			if (pageType == PageType.Home) {
				if (!HasHandledCmdLineArgs) {
					HasHandledCmdLineArgs = true;

					var args = Environment.GetCommandLineArgs();

					if (args.Length > 1) {
						if (Uri.TryCreate(args[1], UriKind.Absolute, out var uri) && uri.Scheme.Equals("uberstrike")) {
							switch (uri.Host.ToLower()) {
								case "connect":
									var _args = uri.AbsolutePath.Substring(1).Split('/');
									var gameServers = GetField<Dictionary<int, PhotonServer>>(Singleton<GameServerManager>.Instance, "_gameServers", BindingFlags.NonPublic | BindingFlags.Instance);

									var photonServer = gameServers.Values.ToList().Find(_ => _.ConnectionString.Equals(_args[0], StringComparison.InvariantCulture));

									if (photonServer != null) {
										Singleton<GameServerController>.Instance.SelectedServer = photonServer;
										Singleton<GameStateController>.Instance.Client.EnterGameLobby(photonServer.ConnectionString);
									} else {
										PopupSystem.ShowMessage("Connection Error", "Could not connect to server.");
										break;
									}

									System.Threading.Timer timer = null;
									timer = new System.Threading.Timer(s => {
										if (Singleton<GameServerController>.Instance.SelectedServer != null) {
											var gameList = GetField<Dictionary<int, GameRoomData>>(Singleton<GameListManager>.Instance, "_gameList", BindingFlags.NonPublic | BindingFlags.Instance);

											var gameServer = gameList.Values.ToList().Find(_ => _.Number == int.Parse(_args[1]));

											if (gameServer != null) {
												Singleton<GameStateController>.Instance.JoinNetworkGame(gameServer);
											} else {
												PopupSystem.ShowMessage("Connection Error", "Could not connect to specified room.");
											}
										}

										timer.Dispose();
									}, null, 200, UInt32.MaxValue - 10);

									break;
								default: break;
							}
						}
					}
				}

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
