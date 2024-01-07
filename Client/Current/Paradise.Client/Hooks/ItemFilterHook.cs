using Cmune.DataCenter.Common.Entities;
using HarmonyLib;
using log4net;
using System;

namespace Paradise.Client {
	[HarmonyPatch]
	public class ItemFilterHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(ItemFilterHook));

		static ItemFilterHook() {
			Log.Info($"[{nameof(ItemFilterHook)}] hooking various item filters");
		}

		[HarmonyPatch(typeof(InventoryItemFilter), "CanPass"), HarmonyPostfix]
		public static void InventoryItemFilter_CanPass_Postfix(ref bool __result, IUnityItem item) {
			__result = __result && CheckRank(item);
		}

		[HarmonyPatch(typeof(ItemByClassFilter), "CanPass"), HarmonyPostfix]
		public static void ItemByClassFilter_CanPass_Postfix(ref bool __result, IUnityItem item) {
			__result = __result && CheckRank(item);
		}

		[HarmonyPatch(typeof(ItemByTypeFilter), "CanPass"), HarmonyPostfix]
		public static void ItemByTypeFilter_CanPass_Postfix(ref bool __result, IUnityItem item) {
			__result = __result && CheckRank(item);
		}

		[HarmonyPatch(typeof(SpecialItemFilter), "CanPass"), HarmonyPostfix]
		public static void SpecialItemFilter_CanPass_Postfix(ref bool __result, IUnityItem item) {
			__result = __result && CheckRank(item);
		}

		private static bool CheckRank(IUnityItem item) {
			
			if (item.View.CustomProperties != null && item.View.CustomProperties.ContainsKey("MinimumRank")) {
				var minimumRank = (MemberAccessLevel)Enum.Parse(typeof(MemberAccessLevel), item.View.CustomProperties["MinimumRank"]);

				return PlayerDataManager.AccessLevel >= minimumRank;
			}

			return true;
		}
	}
}
