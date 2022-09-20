using HarmonyLib;
using log4net;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class GameStateHook : IParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Instantiates prefabs for remotely emitted Quick Items that are not in a local player's inventory.
		/// </summary>
		public GameStateHook() { }

		public void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(GameStateHook)}] hooking {nameof(GameState)}");

			var orig_GameState_EmitRemoteQuickItem = typeof(GameState).GetMethod("EmitRemoteQuickItem", BindingFlags.Public | BindingFlags.Instance);
			var prefix_GameState_EmitRemoteQuickItem = typeof(GameStateHook).GetMethod("EmitRemoteQuickItem_Prefix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_GameState_EmitRemoteQuickItem, new HarmonyMethod(prefix_GameState_EmitRemoteQuickItem), null);
		}

		public static bool EmitRemoteQuickItem_Prefix(GameState __instance, Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID) {
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
