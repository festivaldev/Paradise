using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	///	<br>• Redirects web service/image requests to Paradise Web Services</br>
	/// <br>• Adds updating and custom map functionality</br>
	/// <br>• Adds a debug console (staff only)</br>
	/// <br>• Allows adding additional custom servers to Paradise.Settings.Client.xml</br>
	/// </summary>
	[HarmonyPatch(typeof(ApplicationDataManager))]
	public class ApplicationDataManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ApplicationDataManagerHook));
		private static bool HasPrepared { get; set; }

		private static GameObject PluginHolder;

		static ApplicationDataManagerHook() {
			Log.Info($"[{nameof(ApplicationDataManagerHook)}] hooking {nameof(ApplicationDataManager)}");

			if (PluginHolder == null) {
				foreach (var arg in Environment.GetCommandLineArgs()) {
					switch (arg) {
						case "-console":
							DebugConsoleGUI.Show = true;
							break;
						default: break;
					}
				}

				PluginHolder = new GameObject("Plugin Holder");

				PluginHolder.AddComponent<ParadiseApplicationManager>();
				PluginHolder.AddComponent<ParadiseUpdater>();
				PluginHolder.AddComponent<CustomMapManager>();
				PluginHolder.AddComponent<DebugConsoleGUI>();

				UnityEngine.Object.DontDestroyOnLoad(PluginHolder);
			}
		}

		[HarmonyPatch(MethodType.StaticConstructor), HarmonyPostfix]
		public static void Prepare() {
			var traverse = Traverse.CreateWithType("ApplicationDataManager");
			traverse.Field("WebServiceBaseUrl").SetValue(ParadiseClient.WebServiceBaseUrl);
			traverse.Field("ImagePath").SetValue(ParadiseClient.ImagePath);

			if (ParadiseClient.ServerOverrides != null) {
				if (ParadiseClient.ServerOverrides.CommServer != null) {
					Singleton<GameServerManager>.Instance.CommServer = new PhotonServer(ParadiseClient.ServerOverrides.CommServer);
				}

				if (ParadiseClient.ServerOverrides.GameServers.Count > 0) {
					Singleton<GameServerManager>.Instance.AddPhotonGameServers(ParadiseClient.ServerOverrides.GameServers.FindAll(_ => _.UsageType == PhotonUsageType.All));
				}
			}
		}
	}
}
