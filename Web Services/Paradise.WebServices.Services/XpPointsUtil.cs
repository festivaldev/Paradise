using Newtonsoft.Json;
using Paradise.Core.Models.Views;
using System;
using System.Collections.Generic;
using System.IO;

namespace Paradise.WebServices.Services {
	public static class XpPointsUtil {
		public static string CurrentDirectory => Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

		public static ApplicationConfigurationView Config { get; set; }

		private static readonly FileSystemWatcher watcher;
		private static readonly List<string> watchedFiles = new List<string> {
			"ApplicationConfiguration.json"
		};

		static XpPointsUtil() {
			Config = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(Plugin.ServiceDataPath, "ApplicationWebService", "2.0", "ApplicationConfiguration.json")));

			watcher = new FileSystemWatcher(Path.Combine(Plugin.ServiceDataPath, "ApplicationWebService")) {
				NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
			};

			watcher.Changed += (object sender, FileSystemEventArgs e) => {
				if (!watchedFiles.Contains(e.Name)) return;

				Config = JsonConvert.DeserializeObject<ApplicationConfigurationView>(File.ReadAllText(Path.Combine(Plugin.ServiceDataPath, "ApplicationWebService", "2.0", "ApplicationConfiguration.json")));
			};

			watcher.EnableRaisingEvents = true;
		}

		public static void GetXpRangeForLevel(int level, out int minXp, out int maxXp) {
			level = Math.Min(Math.Max(level, 1), XpPointsUtil.MaxPlayerLevel);

			if (level < MaxPlayerLevel) {
				Config.XpRequiredPerLevel.TryGetValue(level, out minXp);
				Config.XpRequiredPerLevel.TryGetValue(level + 1, out maxXp);
			} else {
				Config.XpRequiredPerLevel.TryGetValue(MaxPlayerLevel, out minXp);
				maxXp = minXp + 1;
			}
		}

		public static int GetLevelForXp(int xp) {
			for (int i = MaxPlayerLevel; i > 0; i--) {
				if (Config.XpRequiredPerLevel.TryGetValue(i, out var num) && xp >= num) {
					return i;
				}
			}

			return 1;
		}

		public static int MaxPlayerLevel {
			get {
				return Config.MaxLevel;
			}
		}
	}
}