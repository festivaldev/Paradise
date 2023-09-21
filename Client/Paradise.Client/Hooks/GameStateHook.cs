using HarmonyLib;
using log4net;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Instantiates prefabs for remotely emitted Quick Items that are not in a local player's inventory.
	/// </summary>
	[HarmonyPatch(typeof(GameState))]
	public class GameStateHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(GameStateHook));

		static GameStateHook() {
			Log.Info($"[{nameof(GameStateHook)}] hooking {nameof(GameState)}");
		}

		[HarmonyPatch("EmitRemoteQuickItem"), HarmonyPrefix]
		public static bool GameState_EmitRemoteQuickItem_Postfix(GameState __instance, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
			IUnityItem itemInShop = Singleton<ItemManager>.Instance.GetItemInShop(itemId);
			if (itemInShop != null) {
				if (!itemInShop.Prefab) {
					var gameObject = itemInShop.Create(Vector3.zero, Quaternion.identity).GetComponent<QuickItem>();
					for (int i = 0; i < gameObject.transform.childCount; i++) {
						gameObject.transform.GetChild(i).gameObject.SetActive(false);
					}
				}
			}

			return true;
		}

		[HarmonyPatch("StartMatch"), HarmonyPostfix]
		public static void GameState_StartMatch_Postfix(GameState __instance, int roundNumber, int endTime) {
			if (endTime == 0) {
				GameState.Current.ResetRoundStartTime();
			}
		}

		[HarmonyPatch("GameStateHelper", "UpdateMatchTime"), HarmonyPrefix]
		public static bool GameStateHelper_UpdateMatchTime_Prefix() {
			if (GameState.Current.RoomData.TimeLimit == 0) {
				GameState.Current.PlayerData.RemainingTime.Value = Mathf.CeilToInt(GameState.Current.GameTime);
				return false;
			}

			return true;
		}
	}
}
