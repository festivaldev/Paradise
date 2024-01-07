using HarmonyLib;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	/// <summary>
	/// Allows players to remove items from their inventory
	/// </summary>
	[HarmonyPatch(typeof(InventoryItemGUI))]
	public class InventoryItemGUIHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(InventoryItemGUIHook));

		private static readonly Dictionary<InventoryItemGUI, ParadiseTraverse<InventoryItemGUI>> traverseByInstance = new Dictionary<InventoryItemGUI, ParadiseTraverse<InventoryItemGUI>>();

		static InventoryItemGUIHook() {
			Log.Info($"[{nameof(InventoryItemGUIHook)}] hooking {nameof(InventoryItemGUI)}");
		}

		[HarmonyPatch("Draw"), HarmonyPrefix]
		public static bool Draw_Prefix(InventoryItemGUI __instance, Rect rect, bool selected) {
			if (!traverseByInstance.TryGetValue(__instance, out var traverse)) {
				traverse = traverseByInstance[__instance] = ParadiseTraverse<InventoryItemGUI>.Create(__instance);
			}

			traverse.InvokeMethod("DrawHighlightedBackground", rect);

			GUI.BeginGroup(rect);

			var item = traverse.GetProperty<IUnityItem>("Item");
			var inventoryItem = traverse.GetProperty<InventoryItem>("InventoryItem");

			traverse.InvokeMethod("DrawIcon", new Rect(4f, 4f, 48f, 48f));
			traverse.InvokeMethod("DrawName", new Rect(63f, 10f, 220f, 20f));
			traverse.InvokeMethod("DrawDaysRemaining", new Rect(63f, 30f, 220f, 20f));

			GUI.contentColor = ColorScheme.UberStrikeRed;
			if (GUITools.Button(new Rect(rect.width - (50f + 4f + 50f + 8f), 7f, 54f, 46f), new GUIContent("Remove"), BlueStonez.buttondark_medium)) {
				PopupSystem.ShowMessage("Delete Item", $"Are you sure you want to remove \"{item.Name}\" from your inventory? To use it, you'll need to purchase it from the shop again.", PopupSystem.AlertType.OKCancel, "DELETE", delegate {
					ParadiseUserWebServiceClient.RemoveItemFromInventory(item.View.ID, PlayerDataManager.AuthToken, delegate (int result) {
						UnityRuntime.StartRoutine(UpdateInventoryAndWallet());
					}, delegate (Exception e) { });
				});
			}
			GUI.contentColor = Color.white;

			if (item.View.ID == 1294) {
				traverse.InvokeMethod("DrawUseButton", new Rect(rect.width - 50f, 7f, 46f, 46f));
			} else if (item.Equippable && (inventoryItem.IsPermanent || inventoryItem.DaysRemaining > 0)) {
				traverse.InvokeMethod("DrawEquipButton", new Rect(rect.width - 50f, 7f, 46f, 46f), LocalizedStrings.Equip);
			} else if (item.View.IsForSale) {
				if (!inventoryItem.IsPermanent) {
					traverse.InvokeMethod("DrawBuyButton", new Rect(rect.width - 50f, 7f, 46f, 46f), LocalizedStrings.Renew, ShopArea.Inventory);
				} else if (inventoryItem.AmountRemaining >= 0) {
					traverse.InvokeMethod("DrawBuyButton", new Rect(rect.width - 50f, 7f, 46f, 46f), LocalizedStrings.Buy, ShopArea.Inventory);
				}
			}

			traverse.InvokeMethod("DrawGrayLine", rect);

			if (selected) {
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				if (item.View.ItemType == UberstrikeItemType.Weapon) {
					GUI.Label(new Rect(12f, 60f, 32f, 32f), UberstrikeIconsHelper.GetIconForItemClass(item.View.ItemClass), GUIStyle.none);
				} else if (item.View.ItemType == UberstrikeItemType.Gear) {
					GUI.Label(new Rect(12f, 60f, 32f, 32f), UberstrikeIconsHelper.GetIconForItemClass(item.View.ItemClass), GUIStyle.none);
				}
				GUI.color = Color.white;

				traverse.InvokeMethod("DrawDescription", new Rect(55f, 60f, 255f, 52f));

				var detailGUI = traverse.GetProperty<IBaseItemDetailGUI>("DetailGUI");
				detailGUI?.Draw();
			}

			GUI.EndGroup();

			return false;
		}

		private static IEnumerator UpdateInventoryAndWallet() {
			var popupDialog = PopupSystem.ShowMessage(LocalizedStrings.UpdatingInventory, LocalizedStrings.WereUpdatingYourInventoryPleaseWait, PopupSystem.AlertType.None);
			yield return UnityRuntime.StartRoutine(Singleton<ItemManager>.Instance.StartGetInventory(false));
			PopupSystem.HideMessage(popupDialog);

			yield return UnityRuntime.StartRoutine(Singleton<PlayerDataManager>.Instance.StartGetMember());
			yield break;
		}
	}
}