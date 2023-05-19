﻿using HarmonyLib;
using log4net;

namespace Paradise.Client {
	/// <summary>
	/// Allows purchasing ingame bundles for free without Steam microtransactions.
	/// </summary>
	[HarmonyPatch(typeof(BundleManager))]
	public class BundleManagerHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(BundleManagerHook));

		static BundleManagerHook() {
			Log.Info($"[{nameof(BundleManagerHook)}] hooking {nameof(BundleManager)}");
		}

		[HarmonyPatch("<BuyBundle>m__A7"), HarmonyPrefix]
		public static bool BundleManager_BuyBundle_m__A7_Prefix(BundleManager __instance, bool success) {
			AccessTools.Method(typeof(BundleManager), "OnMicroTxnCallback").Invoke(__instance, new object[] {
				new Steamworks.MicroTxnAuthorizationResponse_t {
					m_bAuthorized = (byte)(success ? 1 : 0)
				}
			});

			return false;
		}
	}
}
