using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UberStrike.DataCenter.Common.Entities;
using UnityEngine;

namespace Paradise.Client {
	public static class ParadiseClient {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseClient));

		public static ParadiseClientSettings Settings { get; private set; } = new ParadiseClientSettings();

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
				return Settings == null || Settings.AutoUpdates;
			}
		}

		public static AuthenticateApplicationView ServerOverrides {
			get {
				return Settings.ServerOverrides;
			}
		}

		public static void Initialize() {
			using (Stream stream = Assembly.GetAssembly(typeof(ParadiseClient)).GetManifestResourceStream("Paradise.Client.log4net.config")) {
				using (StreamReader reader = new StreamReader(stream)) {
					var logConfig = new XmlDocument();
					logConfig.LoadXml(reader.ReadToEnd());

					XmlConfigurator.Configure(logConfig.DocumentElement);
				}
			}

			Log.Info($"Initializing Paradise (Version {Assembly.GetExecutingAssembly().GetName().Version})");

			XmlSerializer ser = new XmlSerializer(typeof(ParadiseClientSettings));

			if (!File.Exists(ParadiseClientSettings.SettingsFilename)) {
				using (TextWriter writer = new StreamWriter(ParadiseClientSettings.SettingsFilename)) {
					ser.Serialize(writer, Settings);
				}
			} else {
				using (XmlReader reader = XmlReader.Create(Path.Combine(Application.dataPath, "Paradise.Settings.Client.xml"))) {
					try {
						Settings = (ParadiseClientSettings)ser.Deserialize(reader);
					} catch (Exception e) {
						Log.Error($"Error while loading Paradise settings: {e.Message}");
						Log.Debug(e);
					}
				}
			}
		}

		private static string ForceTrailingSlash(string uri) {
			return uri.EndsWith("/") ? uri : uri + "/";
		}
	}
}
