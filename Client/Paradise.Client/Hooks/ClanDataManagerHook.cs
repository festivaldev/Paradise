﻿using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;

namespace Paradise.Client {
	/// <summary>
	/// Disables client-side clan requirements for Admins.
	/// </summary>
	[HarmonyPatch(typeof(ClanDataManager))]
	public class ClanDataManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ClanDataManagerHook));

		static ClanDataManagerHook() {
			Log.Info($"[{nameof(ClanDataManagerHook)}] hooking {nameof(ClanDataManager)}");
		}

		[HarmonyPatch("get_HaveFriends"), HarmonyPostfix]
		public static void get_HaveFriends_Postfix(ref bool __result) {
			if (PlayerDataManager.AccessLevel == MemberAccessLevel.Admin) {
				__result = true;
			}
		}

		[HarmonyPatch("get_HaveLevel"), HarmonyPostfix]
		public static void get_HaveLevel_Postfix(ref bool __result) {
			if (PlayerDataManager.AccessLevel == MemberAccessLevel.Admin) {
				__result = true;
			}
		}

		[HarmonyPatch("get_HaveLicense"), HarmonyPostfix]
		public static void get_HaveLicense_Postfix(ref bool __result) {
			if (PlayerDataManager.AccessLevel == MemberAccessLevel.Admin) {
				__result = true;
			}
		}
	}
}
