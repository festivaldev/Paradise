using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UberStrike.DataCenter.Common.Entities;
using UnityEngine;

namespace Paradise.Client {
	public class ApplicationDataManagerHook : IParadiseHook {
		private static ParadiseSettings Settings;
		private static GameObject PluginHolder;

		public static string WebServiceBaseUrl {
			get {
				return ForceTrailingSlash(Settings.WebServiceBaseUrl) ?? "https://ws.uberstrike.com/2.0/";
			}
		}
		public static string WebServicePrefix {
			get {
				return Settings.WebServicePrefix ?? "UberStrike.DataCenter.WebService.CWS.";
			}
		}
		public static string WebServiceSuffix {
			get {
				return Settings.WebServiceSuffix ?? "Contract.svc";
			}
		}

		public static string ImagePath {
			get {
				return ForceTrailingSlash(Settings.ImagePath) ?? "https://static.uberstrike.com/images/";
			}
		}

		public static string UpdateUrl {
			get {
				return ForceTrailingSlash(Settings.UpdateUrl) ?? "https://ws.uberstrike.com/2.0/";
			}
		}
		public static UpdateChannel UpdateChannel {
			get {
				return Settings.UpdateChannel;
			}
		}
		public static bool AutoUpdates {
			get {
				return (Settings != null) ? Settings.AutoUpdates : true;
			}
		}

		private static AuthenticateApplicationView ServerOverrides {
			get {
				return Settings.ServerOverrides;
			}
		}

		public ApplicationDataManagerHook() {
			XmlSerializer ser = new XmlSerializer(typeof(ParadiseSettings));

			using (XmlReader reader = XmlReader.Create(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "UberStrike_Data\\ParadiseSettings.Client.xml")))) {
				try {
					Settings = (ParadiseSettings)ser.Deserialize(reader);
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

			if (ServerOverrides != null) {
				if (ServerOverrides.CommServer != null) {
					Singleton<GameServerManager>.Instance.CommServer = new PhotonServer(ServerOverrides.CommServer);
				}

				if (ServerOverrides.GameServers.Count > 0) {
					Singleton<GameServerManager>.Instance.AddPhotonGameServers(ServerOverrides.GameServers.FindAll(_ => _.UsageType == PhotonUsageType.All));
				}
			}
		}

		private static string ForceTrailingSlash(string uri) {
			return uri.EndsWith("/") ? uri : uri + "/";
		}
	}
}
