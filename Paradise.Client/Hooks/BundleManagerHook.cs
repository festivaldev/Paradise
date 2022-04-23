using HarmonyLib;
using System;
using System.Reflection;

namespace Paradise.Client {
	public class BundleManagerHook : IParadiseHook {
		private static BundleManager Instance;

		public Type TypeToHook => typeof(BundleManager);

		public void Hook() {
			var harmony = new Harmony("tf.festival.Paradise.BundleManagerHook");

			var orig_BundleManager_Initialize = typeof(BundleManager).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
			var prefix_BundleManager_Initialize = typeof(BundleManagerHook).GetMethod("Initialize_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmony.Patch(orig_BundleManager_Initialize, new HarmonyMethod(prefix_BundleManager_Initialize), null);

			var orig_BundleManager_BuyBundle_m__A7 = typeof(BundleManager).GetMethod("<BuyBundle>m__A7", BindingFlags.Static | BindingFlags.NonPublic);
			var prefix_BundleManager_BuyBundle_m__A7 = typeof(BundleManagerHook).GetMethod("BuyBundle_m__A7_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmony.Patch(orig_BundleManager_BuyBundle_m__A7, new HarmonyMethod(prefix_BundleManager_BuyBundle_m__A7), null);
		}

		public static bool Initialize_Prefix(BundleManager __instance) {
			Instance = __instance;

			return true;
		}

		public static bool BuyBundle_m__A7_Prefix(bool success) {
			typeof(BundleManager).GetMethod("OnMicroTxnCallback", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Instance, new object[] {
				new Steamworks.MicroTxnAuthorizationResponse_t {
					m_bAuthorized = (byte)(success ? 1 : 0)
				}
			});

			return false;
		}
	}
}
