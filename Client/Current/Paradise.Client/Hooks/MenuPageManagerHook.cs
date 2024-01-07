using HarmonyLib;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UberStrike.Core.Models;

namespace Paradise.Client {
	/// <summary>
	/// <br>• Adds game update logic (checked on game startup and every 60 minutes when entering the main menu) and displays a warning if updates are disabled.</br>
	/// <br>• Responsible for requesting custom maps from server once on game startup.</br>
	/// </summary>
	[HarmonyPatch(typeof(MenuPageManager))]
	public class MenuPageManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(MenuPageManagerHook));

		private static bool HasHandledCmdLineArgs;
		private static bool HasRequestedMaps;

		static MenuPageManagerHook() {
			Log.Info($"[{nameof(MenuPageManagerHook)}] hooking {nameof(MenuPageManager)}");
		}

		[HarmonyPatch("LoadPage"), HarmonyPrefix]
		public static bool LoadPage_Prefix(MenuPageManager __instance, PageType pageType, bool forceReload = false) {
			AutoMonoBehaviour<PreloadOptionsPanelButton>.Instance.enabled = false;

			if (pageType == PageType.Home) {
				AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
				AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.HomeSceneBackground);

				if (!HasHandledCmdLineArgs) {
					HasHandledCmdLineArgs = true;

					var args = Environment.GetCommandLineArgs();

					if (args.Length > 1) {
						if (Uri.TryCreate(args[1], UriKind.Absolute, out var uri) && uri.Scheme.Equals("uberstrike")) {
							using (var menuTimer = new Timer(menuTimerState => {
								switch (uri.Host.ToLower()) {
									case "connect":
										var _args = uri.AbsolutePath.Substring(1).Split('/');
										var gameServers = Traverse.Create(Singleton<GameServerManager>.Instance).Field<Dictionary<int, PhotonServer>>("_gameServers").Value;

										var photonServer = gameServers.Values.ToList().Find(_ => _.ConnectionString.Equals(_args[0], StringComparison.InvariantCulture));

										if (photonServer != null) {
											Singleton<GameServerController>.Instance.SelectedServer = photonServer;
											Singleton<GameStateController>.Instance.Client.EnterGameLobby(photonServer.ConnectionString);
										} else {
											PopupSystem.ShowMessage("Connection Error", "Could not connect to server.");
											break;
										}

										using (var photonConnectTimer = new Timer(photonConnectState => {
											if (Singleton<GameServerController>.Instance.SelectedServer != null) {
												var gameList = Traverse.Create(Singleton<GameListManager>.Instance).Field<Dictionary<int, GameRoomData>>("_gameList").Value;

												var gameServer = gameList.Values.ToList().Find(_ => _.Number == int.Parse(_args[1]));

												if (gameServer != null) {
													Singleton<GameStateController>.Instance.JoinNetworkGame(gameServer);
												} else {
													PopupSystem.ShowMessage("Connection Error", "Could not connect to specified room.");
												}
											}
										}, null, 200, Timeout.Infinite)) { }

										break;
									case "open":
										var page = uri.Segments[1];

										switch (page) {
											case "play":
												GameData.Instance.MainMenu.Value = MainMenuState.None;
												__instance.LoadPage(PageType.Play);

												break;
											case "stats":
												GameData.Instance.MainMenu.Value = MainMenuState.None;
												__instance.LoadPage(PageType.Stats);

												break;
											case "shop":
												GameData.Instance.MainMenu.Value = MainMenuState.None;
												__instance.LoadPage(PageType.Shop);

												break;
											case "inbox":
												GameData.Instance.MainMenu.Value = MainMenuState.None;
												__instance.LoadPage(PageType.Inbox);

												break;
											case "clans":
												GameData.Instance.MainMenu.Value = MainMenuState.None;
												__instance.LoadPage(PageType.Clans);

												break;
											case "training":
												GameData.Instance.MainMenu.Value = MainMenuState.None;
												__instance.LoadPage(PageType.Training);

												break;
											case "chat":
												GameData.Instance.MainMenu.Value = MainMenuState.None;
												__instance.LoadPage(PageType.Chat);

												break;
										}

										break;
									default: break;
								}
							}, null, 0, Timeout.Infinite)) { }
						}
					}
				}

				if (!HasRequestedMaps) {
					HasRequestedMaps = true;
					UnityRuntime.StartRoutine(CustomMapManager.GetCustomMaps());
				}

				UnityRuntime.StartRoutine(AutoMonoBehaviour<ParadiseUpdater>.Instance.CheckForUpdatesIfNecessary(
					ParadiseUpdater.HandleUpdateAvailable,
					ParadiseUpdater.HandleUpdateError
				));
			}

			return true;
		}
	}
}
