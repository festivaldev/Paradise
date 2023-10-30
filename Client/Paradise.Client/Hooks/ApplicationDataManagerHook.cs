using HarmonyLib;
using log4net;
using System;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	///	<br>• Redirects web service/image requests to Paradise Web Services</br>
	/// <br>• Adds updating and custom map functionality</br>
	/// <br>• Adds a debug console</br>
	/// </summary>
	[HarmonyPatch(typeof(ApplicationDataManager))]
	public class ApplicationDataManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ApplicationDataManagerHook));

		private static readonly ParadiseTraverse traverse;
		private static readonly GameObject PluginHolder;

		private static bool hasPrepared;

		static ApplicationDataManagerHook() {
			Log.Info($"[{nameof(ApplicationDataManagerHook)}] hooking {nameof(ApplicationDataManager)}");

			traverse = ParadiseTraverse.Create(typeof(ApplicationDataManager));

			if (!PluginHolder) {
				PluginHolder = new GameObject("Plugin Holder");
				PluginHolder.AddComponent<ParadiseApplicationManager>();

				UnityEngine.Object.DontDestroyOnLoad(PluginHolder);
			}
		}

		[HarmonyPatch(MethodType.StaticConstructor), HarmonyPostfix]
		public static void Prepare() {
			if (hasPrepared) return;
			hasPrepared = true;

			var webServiceUri = new UriBuilder(ParadiseClient.Settings.WebServiceBaseUrl) {
				Path = ParadiseClient.Settings.WebServiceEndpoint
			}.ToString();

			var imagePathUri = new UriBuilder(ParadiseClient.Settings.FileServerUrl) {
				Path = ParadiseClient.Settings.ImagePathEndpoint
			}.ToString();


			traverse.SetField("WebServiceBaseUrl", ForceTrailingSlash(webServiceUri));
			traverse.SetField("ImagePath", ForceTrailingSlash(imagePathUri));
		}

		private static string ForceTrailingSlash(string uri) {
			return uri.EndsWith("/") ? uri : uri + "/";
		}
	}
}
