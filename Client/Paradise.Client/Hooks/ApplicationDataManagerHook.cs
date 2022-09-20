using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class ApplicationDataManagerHook : IParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		private static GameObject PluginHolder;

		/// <summary>
		///	<br>• Redirects web service/image requests to Paradise Web Services</br>
		/// <br>• Adds updating and custom map functionality</br>
		/// <br>• Adds a debug console (staff only)</br>
		/// <br>• Allows adding additional custom servers to Paradise.Settings.Client.xml</br>
		/// </summary>
		public ApplicationDataManagerHook() {
			PluginHolder = new GameObject("Plugin Holder");

			PluginHolder.AddComponent<ParadiseApplicationManager>();
			PluginHolder.AddComponent<ParadiseUpdater>();
			PluginHolder.AddComponent<CustomMapManager>();
			PluginHolder.AddComponent<DebugConsoleGUI>();

			UnityEngine.Object.DontDestroyOnLoad(PluginHolder);
		}

		public void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(ApplicationDataManagerHook)}] hooking {nameof(ApplicationDataManager)}");

			var type = typeof(ApplicationDataManager);

			var WebServiceBaseUrl_field = type.GetField("WebServiceBaseUrl", BindingFlags.Public | BindingFlags.Static);
			WebServiceBaseUrl_field.SetValue(null, ParadiseClient.WebServiceBaseUrl);

			var ImagePath_field = type.GetField("ImagePath", BindingFlags.Public | BindingFlags.Static);
			ImagePath_field.SetValue(null, ParadiseClient.ImagePath);

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
