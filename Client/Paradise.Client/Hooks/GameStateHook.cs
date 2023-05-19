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
		public static bool GameState_EmitRemoteQuickItem_Prefix(GameState __instance, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
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
	}
}
