using Paradise.Core.Models.Views;
using System;

namespace Paradise.Realtime.Server.Game {
	public static class XpPointsUtil {
		public static ApplicationConfigurationView Config { get; set; }

		private static readonly System.Timers.Timer UpdateTimer;

		static XpPointsUtil() {
			Config = ApplicationWebServiceClient.Instance.GetConfigurationData("4.7.1");

			UpdateTimer = new System.Timers.Timer(TimeSpan.FromMinutes(15).TotalMilliseconds);
			UpdateTimer.Elapsed += delegate {
				Config = ApplicationWebServiceClient.Instance.GetConfigurationData("4.7.1");
			};
			UpdateTimer.Start();
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