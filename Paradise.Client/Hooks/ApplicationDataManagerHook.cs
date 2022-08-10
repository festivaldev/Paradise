using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Paradise.Client {
	public class ApplicationDataManagerHook : IParadiseHook {
		public static string WebServiceBaseUrl { get; private set; } = "https://ws.uberstrike.com/2.0/";
		public static string WebServicePrefix { get; private set; } = "UberStrike.DataCenter.WebService.CWS.";
		public static string WebServiceSuffix { get; private set; } = "Contract.svc";

		public static string ImagePath { get; private set; } = "https://static.uberstrike.com/images/";

		public static string UpdateUrl { get; private set; } = "https://localhost:8081/updates/";
		public static string UpdateChannel { get; private set; } = "stable";
		public static bool AutoUpdates { get; private set; } = true;

		public static GameObject PluginHolder;

		public ApplicationDataManagerHook() {
			XmlSerializer ser = new XmlSerializer(typeof(ParadiseSettings));

			using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "UberStrike_Data\\ParadiseSettings.Client.xml")))) {
				try {
					var settings = (ParadiseSettings)ser.Deserialize(reader);

					WebServiceBaseUrl = ForceTrailingSlash(settings.WebServiceBaseUrl);
					WebServicePrefix = settings.WebServicePrefix;
					WebServiceSuffix = settings.WebServiceSuffix;

					ImagePath = ForceTrailingSlash(settings.ImagePath);

					UpdateUrl = ForceTrailingSlash(settings.UpdateUrl);
					UpdateChannel = (settings.UpdateChannel ?? UpdateChannel).ToLower();
					AutoUpdates = settings.AutoUpdates;
				} catch (Exception e) {
					Debug.LogError($"Error while loading Paradise settings: {e}");
				}
			}

			PluginHolder = new GameObject("Plugin Holder");

			PluginHolder.AddComponent<ParadiseApplicationManager>();
			PluginHolder.AddComponent<ParadiseUpdater>();
			PluginHolder.AddComponent<CustomMapManager>();
			PluginHolder.AddComponent<DebugConsoleGUI>();

			UnityEngine.Object.DontDestroyOnLoad(PluginHolder);
		}

		public void Hook(Harmony harmonyInstance) {
			Debug.Log($"[{typeof(ApplicationDataManagerHook)}] hooking {typeof(ApplicationDataManager)}");

			var type = typeof(ApplicationDataManager);

			var WebServiceBaseUrl_field = type.GetField("WebServiceBaseUrl", BindingFlags.Public | BindingFlags.Static);
			WebServiceBaseUrl_field.SetValue(null, WebServiceBaseUrl);

			var ImagePath_field = type.GetField("ImagePath", BindingFlags.Public | BindingFlags.Static);
			ImagePath_field.SetValue(null, ImagePath);
		}

		private static string ForceTrailingSlash(string uri) {
			return uri.EndsWith("/") ? uri : uri + "/";
		}
	}
}
