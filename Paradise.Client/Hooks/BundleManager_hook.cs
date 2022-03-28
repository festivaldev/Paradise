using HarmonyLib;
using System.Reflection;

namespace Paradise.Client {
	public static class BundleManager_hook {
		private static BundleManager Instance;

		public static void Hook() {
			var harmony = new Harmony("tf.festival.Paradise.Patch_BundleManager");

			var orig_BundleManager_Initialize = typeof(BundleManager).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
			var prefix_BundleManager_Initialize = typeof(BundleManager_hook).GetMethod("Initialize_Prefix", BindingFlags.Static | BindingFlags.Public);

			harmony.Patch(orig_BundleManager_Initialize, new HarmonyMethod(prefix_BundleManager_Initialize), null);

			var orig_BundleManager_BuyBundle_m__A7 = typeof(BundleManager).GetMethod("<BuyBundle>m__A7", BindingFlags.NonPublic | BindingFlags.Static);
			var prefix_BundleManager_BuyBundle_m__A7 = typeof(BundleManager_hook).GetMethod("BuyBundle_m__A7_Prefix", BindingFlags.Static | BindingFlags.Public);

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
