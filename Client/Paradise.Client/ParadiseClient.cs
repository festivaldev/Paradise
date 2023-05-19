using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UberStrike.DataCenter.Common.Entities;
using UnityEngine;

namespace Paradise.Client {
	public static class ParadiseClient {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ParadiseClient));

		public static ParadiseClientSettings Settings { get; private set; } = new ParadiseClientSettings();

		public static bool EnableDiscordRichPresence {
			get {
				return Settings == null || Settings.EnableDiscordRichPresence;
			}
		}
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

		static ParadiseClient() {
			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
				var assemblyName = new AssemblyName(e.Name).Name;
				var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(_ => _.EndsWith($"{assemblyName}.dll"));

				if (string.IsNullOrEmpty(resourceName)) return null;

				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
					return Assembly.Load(new BinaryReader(stream).ReadBytes((int)stream.Length));
				}
			};
		}

		public static void Initialize() {
			using (Stream stream = Assembly.GetAssembly(typeof(ParadiseClient)).GetManifestResourceStream("Paradise.Client.log4net.config")) {
				using (StreamReader reader = new StreamReader(stream)) {
					var logConfig = new XmlDocument();
					logConfig.LoadXml(reader.ReadToEnd());

					XmlConfigurator.Configure(logConfig.DocumentElement);
				}
			}

			Log.Info($"Initializing Paradise (Version {Assembly.GetExecutingAssembly().GetName().Version}) ({Application.platform})");

			XmlSerializer ser = new XmlSerializer(typeof(ParadiseClientSettings));

			if (!File.Exists(ParadiseClientSettings.SettingsFilename)) {
				using (TextWriter writer = new StreamWriter(ParadiseClientSettings.SettingsFilename)) {
					ser.Serialize(writer, Settings);
				}
			} else {
				using (XmlReader reader = XmlReader.Create(Path.Combine(Application.dataPath, "Paradise.Settings.Client.xml"), new XmlReaderSettings { IgnoreComments = true })) {
					try {
						Settings = (ParadiseClientSettings)ser.Deserialize(reader);
					} catch (Exception e) {
						Log.Error($"Error while loading Paradise settings: {e.Message}");
						Log.Debug(e);
					}
				}
			}

			RichPresenceClient.Initialize();
		}

		private static string ForceTrailingSlash(string uri) {
			return uri.EndsWith("/") ? uri : uri + "/";
		}
	}
}
