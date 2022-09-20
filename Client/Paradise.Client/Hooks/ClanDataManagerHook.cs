using HarmonyLib;
using log4net;
using System.Reflection;

namespace Paradise.Client {
	public class ClanDataManagerHook : IParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Disables clientside clan requirements for debugging purposes.
		/// </summary>
		public ClanDataManagerHook() { }

		public void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(ClanDataManagerHook)}] hooking {nameof(ClanDataManager)}");

			var orig_ClanDataManager_get_HaveFriends = typeof(ClanDataManager).GetMethod("get_HaveFriends", BindingFlags.Public | BindingFlags.Instance);
			var postfix_ClanDataManager_get_HaveFriends = typeof(ClanDataManagerHook).GetMethod("get_HaveFriends_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_ClanDataManager_get_HaveFriends, null, new HarmonyMethod(postfix_ClanDataManager_get_HaveFriends));

			var orig_ClanDataManager_get_HaveLevel = typeof(ClanDataManager).GetMethod("get_HaveLevel", BindingFlags.Public | BindingFlags.Instance);
			var postfix_ClanDataManager_get_HaveLevel = typeof(ClanDataManagerHook).GetMethod("get_HaveLevel_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_ClanDataManager_get_HaveLevel, null, new HarmonyMethod(postfix_ClanDataManager_get_HaveLevel));

			var orig_ClanDataManager_get_HaveLicense = typeof(ClanDataManager).GetMethod("get_HaveLicense", BindingFlags.Public | BindingFlags.Instance);
			var postfix_ClanDataManager_get_HaveLicense = typeof(ClanDataManagerHook).GetMethod("get_HaveLicense_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_ClanDataManager_get_HaveLicense, null, new HarmonyMethod(postfix_ClanDataManager_get_HaveLicense));
		}

		public static void get_HaveFriends_Postfix(ref bool __result) {
			__result = true;
		}

		public static void get_HaveLevel_Postfix(ref bool __result) {
			__result = true;
		}

		public static void get_HaveLicense_Postfix(ref bool __result) {
			__result = true;
		}
	}
}
