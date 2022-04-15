using Paradise.Core.Models.Views;
using Paradise.WebServices.Client;
using System;

namespace Paradise.Realtime.Server.Game {
	public static class XpPointsUtil {
		public static ApplicationConfigurationView Config { get; set; }

		static XpPointsUtil() {
			Config = new ApplicationWebServiceClient(GameApplication.Instance.Configuration.WebServiceBaseUrl).GetConfigurationData("4.7.1");
		}

		public static void GetXpRangeForLevel(int level, out int minXp, out int maxXp) {
			level = Math.Min(Math.Max(level, 1), XpPointsUtil.MaxPlayerLevel);
			minXp = 0;
			maxXp = 0;

			if (level < XpPointsUtil.MaxPlayerLevel) {
				XpPointsUtil.Config.XpRequiredPerLevel.TryGetValue(level, out minXp);
				XpPointsUtil.Config.XpRequiredPerLevel.TryGetValue(level + 1, out maxXp);
			} else {
				XpPointsUtil.Config.XpRequiredPerLevel.TryGetValue(XpPointsUtil.MaxPlayerLevel, out minXp);
				maxXp = minXp + 1;
			}
		}

		public static int GetLevelForXp(int xp) {
			for (int i = XpPointsUtil.MaxPlayerLevel; i > 0; i--) {
				int num;
				if (XpPointsUtil.Config.XpRequiredPerLevel.TryGetValue(i, out num) && xp >= num) {
					return i;
				}
			}

			return 1;
		}

		public static int MaxPlayerLevel {
			get {
				return XpPointsUtil.Config.MaxLevel;
			}
		}
	}
}