using Cmune.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UberStrike.Core.Types;
using UnityEngine;

namespace Paradise.Client {
	internal class DebugShopPanel : IDebugPage {
		public string Title => "Shop";

		private int selectedTab;
		private readonly string[] tabs = { "Shop", "Inventory", "Loadout" };

		private readonly ParadiseTraverse traverse = ParadiseTraverse.Create(Singleton<InventoryManager>.Instance);

		private readonly SelectionGroup<UberstrikeItemType> typeSelection = new SelectionGroup<UberstrikeItemType>();
		private readonly SelectionGroup<UberstrikeItemClass> weaponClassSelection = new SelectionGroup<UberstrikeItemClass>();
		private readonly SelectionGroup<UberstrikeItemClass> gearClassSelection = new SelectionGroup<UberstrikeItemClass>();
		private IShopItemFilter itemFilter;

		private readonly string[] weaponSlotNames = {
			LocalizedStrings.Melee,
			LocalizedStrings.PrimaryWeapon,
			LocalizedStrings.SecondaryWeapon,
			LocalizedStrings.TertiaryWeapon
		};

		private readonly string[] quickSlotNames = {
			$"{LocalizedStrings.QuickItem} 1",
			$"{LocalizedStrings.QuickItem} 2",
			$"{LocalizedStrings.QuickItem} 3"
		};

		public DebugShopPanel() {
			typeSelection.Add(UberstrikeItemType.Weapon, new GUIContent(ShopIcons.WeaponItems, LocalizedStrings.Weapons));
			typeSelection.Add(UberstrikeItemType.Gear, new GUIContent(ShopIcons.GearItems, LocalizedStrings.Gear));
			typeSelection.Add(UberstrikeItemType.QuickUse, new GUIContent(ShopIcons.QuickItems, LocalizedStrings.QuickItems));
			typeSelection.Add(UberstrikeItemType.Functional, new GUIContent(ShopIcons.FunctionalItems, LocalizedStrings.FunctionalItems));

			typeSelection.SetIndex(0);

			typeSelection.OnSelectionChange += delegate (UberstrikeItemType itemType) {
				UpdateItemFilter();
			};

			weaponClassSelection.Add(UberstrikeItemClass.WeaponMelee, new GUIContent(ShopIcons.StatsMostWeaponSplatsMelee, LocalizedStrings.MeleeWeapons));
			weaponClassSelection.Add((UberstrikeItemClass)2, new GUIContent(ShopIcons.StatsMostWeaponSplatsHandgun, "Handguns"));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponMachinegun, new GUIContent(ShopIcons.StatsMostWeaponSplatsMachinegun, LocalizedStrings.Machineguns));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponShotgun, new GUIContent(ShopIcons.StatsMostWeaponSplatsShotgun, LocalizedStrings.Shotguns));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponSniperRifle, new GUIContent(ShopIcons.StatsMostWeaponSplatsSniperRifle, LocalizedStrings.SniperRifles));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponCannon, new GUIContent(ShopIcons.StatsMostWeaponSplatsCannon, LocalizedStrings.Cannons));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponSplattergun, new GUIContent(ShopIcons.StatsMostWeaponSplatsSplattergun, LocalizedStrings.Splatterguns));
			weaponClassSelection.Add(UberstrikeItemClass.WeaponLauncher, new GUIContent(ShopIcons.StatsMostWeaponSplatsLauncher, LocalizedStrings.Launchers));

			weaponClassSelection.SetIndex(-1);

			weaponClassSelection.OnSelectionChange += delegate (UberstrikeItemClass itemClass) {
				UpdateItemFilter();
			};

			gearClassSelection.Add(UberstrikeItemClass.GearBoots, new GUIContent(ShopIcons.Boots, LocalizedStrings.Boots));
			gearClassSelection.Add(UberstrikeItemClass.GearHead, new GUIContent(ShopIcons.Head, LocalizedStrings.Head));
			gearClassSelection.Add(UberstrikeItemClass.GearFace, new GUIContent(ShopIcons.Face, LocalizedStrings.Face));
			gearClassSelection.Add(UberstrikeItemClass.GearUpperBody, new GUIContent(ShopIcons.Upperbody, LocalizedStrings.UpperBody));
			gearClassSelection.Add(UberstrikeItemClass.GearLowerBody, new GUIContent(ShopIcons.Lowerbody, LocalizedStrings.LowerBody));
			gearClassSelection.Add(UberstrikeItemClass.GearGloves, new GUIContent(ShopIcons.Gloves, LocalizedStrings.Gloves));
			gearClassSelection.Add(UberstrikeItemClass.GearHolo, new GUIContent(ShopIcons.Holos, LocalizedStrings.Holo));

			gearClassSelection.SetIndex(-1);

			gearClassSelection.OnSelectionChange += delegate (UberstrikeItemClass itemClass) {
				UpdateItemFilter();
			};

			UpdateItemFilter();
		}

		public void Draw() {
			selectedTab = GUILayout.Toolbar(selectedTab, tabs, BlueStonez.tab_medium);
			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			ParadiseGUITools.DrawGroup(tabs[selectedTab], delegate {
				switch (selectedTab) {
					case 0:
						DrawShopItems();
						break;
					case 1:
						DrawInventoryItems();
						break;
					case 2:
						DrawLoadoutItems();
						break;
				}
			});
		}

		private void DrawShopItems() {
			var selection = GUILayout.Toolbar(typeSelection.Index, typeSelection.GuiContent, BlueStonez.tab_medium, GUILayout.Height(32f));
			if (selection != typeSelection.Index) {
				typeSelection.SetIndex(selection);
				AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.ButtonClick, 0UL, 1f, 1f);
			}

			switch (typeSelection.Current) {
				case UberstrikeItemType.Weapon: {
					GUI.changed = false;
					var classSelection = GUILayout.Toolbar(weaponClassSelection.Index, weaponClassSelection.GuiContent, BlueStonez.tab_medium, GUILayout.Height(32f));

					if (GUI.changed) {
						if (classSelection == weaponClassSelection.Index) {
							weaponClassSelection.SetIndex(-1);
						} else {
							weaponClassSelection.SetIndex(classSelection);
						}

						AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.ButtonClick, 0UL, 1f, 1f);
					}

					break;
				}
				case UberstrikeItemType.Gear: {
					GUI.changed = false;
					var classSelection = GUILayout.Toolbar(gearClassSelection.Index, gearClassSelection.GuiContent, BlueStonez.tab_medium, GUILayout.Height(32f));

					if (GUI.changed) {
						if (classSelection == gearClassSelection.Index) {
							gearClassSelection.SetIndex(-1);
						} else {
							gearClassSelection.SetIndex(classSelection);
						}

						AutoMonoBehaviour<SfxManager>.Instance.Play2dAudioClip(GameAudio.ButtonClick, 0UL, 1f, 1f);
					}

					break;
				}
				case UberstrikeItemType.QuickUse:
					break;
				case UberstrikeItemType.Functional:
					break;
			}

			var shopItems = Singleton<ItemManager>.Instance.ShopItems.Where(_ => !Singleton<ItemManager>.Instance.IsDefaultGearItem(_.View.PrefabName) && itemFilter.CanPass(_));

			foreach (var item in shopItems.Select((x, i) => new { Value = x, Index = i })) {
				if (item.Index > 0) {
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				}

				var shopItem = item.Value;

				GUI.enabled = !Singleton<InventoryManager>.Instance.Contains(shopItem.View.ID);
				if (GUILayout.Button($"{shopItem.Name} ({shopItem.View.ID})", BlueStonez.buttondark_small, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
					if (PlayerDataManager.AccessLevel == MemberAccessLevel.Admin) {
						traverse.GetField<IDictionary<int, InventoryItem>>("_inventoryItems").Add(shopItem.View.ID, new InventoryItem(shopItem) {
							IsPermanent = true,
							AmountRemaining = -1,
							ExpirationDate = new DateTime?(DateTime.MaxValue)
						});
					}
				}
				GUI.enabled = true;

				var rect = GUILayoutUtility.GetLastRect();
				if (rect.Contains(Event.current.mousePosition)) {
					AutoMonoBehaviour<ItemToolTip>.Instance.SetItem(shopItem, rect, PopupViewSide.Left, -1, BuyingDurationType.None);
				}
			}
		}

		private void DrawInventoryItems() {
			foreach (var item in Singleton<InventoryManager>.Instance.InventoryItems.Select((x, i) => new { Value = x, Index = i })) {
				if (item.Index > 0) {
					GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);
				}

				var inventoryItem = item.Value;

				GUILayout.Label($"{inventoryItem.Item.Name} ({inventoryItem.Item.View.ID})", BlueStonez.label_interparkbold_11pt_left);
				ParadiseGUITools.DrawTextField("Amount Remaining", inventoryItem.AmountRemaining);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);
				ParadiseGUITools.DrawTextField("Days Remaining", inventoryItem.DaysRemaining);
				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				var loadoutSlotType = Singleton<LoadoutManager>.Instance.Loadout.GetItemClassSlotType(inventoryItem.Item.View.ItemClass);

				GUI.enabled = !Singleton<LoadoutManager>.Instance.IsItemEquipped(inventoryItem.Item.View.ID);
				if (GUILayout.Button(LocalizedStrings.Equip, BlueStonez.buttondark_medium, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
					Debug.Log($"{loadoutSlotType} {inventoryItem.Item}");
					Singleton<InventoryManager>.Instance.EquipItem(inventoryItem.Item);
					GameState.Current.Avatar.HideWeapons();
				}
				GUI.enabled = true;
			}
		}

		private void DrawLoadoutItems() {
			foreach (var item in LoadoutManager.GearSlots.Select((x, i) => new { Value = x, Index = i })) {
				var loadoutSlotType = item.Value;

				if (item.Index > 0) GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Label(LoadoutManager.GearSlotNames[item.Index], BlueStonez.label_interparkbold_13pt_left);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				if (Singleton<LoadoutManager>.Instance.Loadout.TryGetItem(loadoutSlotType, out var shopItem) &&
				!Singleton<ItemManager>.Instance.IsDefaultGearItem(shopItem.View.PrefabName)) {
					GUILayout.Label($"{shopItem.View.Name} ({shopItem.View.ID})", BlueStonez.label_interparkbold_11pt_left);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					if (GUILayout.Button(LocalizedStrings.Unequip, BlueStonez.buttondark_medium, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
						Singleton<LoadoutManager>.Instance.ResetSlot(loadoutSlotType);
					}
				} else {
					GUI.enabled = false;
					GUILayout.Label("No item equipped", BlueStonez.label_interparkbold_11pt_left);
					GUI.enabled = true;
				}
			}

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			foreach (var item in LoadoutManager.WeaponSlots.Select((x, i) => new { Value = x, Index = i })) {
				var loadoutSlotType = item.Value;

				if (item.Index > 0) GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Label(weaponSlotNames[item.Index], BlueStonez.label_interparkbold_13pt_left);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				if ((Singleton<LoadoutManager>.Instance.Loadout.TryGetItem(loadoutSlotType, out var shopItem)) &&
				!Singleton<ItemManager>.Instance.IsDefaultGearItem(shopItem.View.PrefabName)) {
					GUILayout.Label($"{shopItem.View.Name} ({shopItem.View.ID})", BlueStonez.label_interparkbold_11pt_left);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					if (GUILayout.Button(LocalizedStrings.Unequip, BlueStonez.buttondark_medium, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
						Singleton<LoadoutManager>.Instance.ResetSlot(loadoutSlotType);
					}
				} else {
					GUI.enabled = false;
					GUILayout.Label("No item equipped", BlueStonez.label_interparkbold_11pt_left);
					GUI.enabled = true;
				}
			}

			GUILayout.Space(ParadiseGUITools.SECTION_SPACING);

			foreach (var item in LoadoutManager.QuickSlots.Select((x, i) => new { Value = x, Index = i })) {
				var loadoutSlotType = item.Value;

				if (item.Index > 0) GUILayout.Space(ParadiseGUITools.ITEM_SPACING_V);

				GUILayout.Label(quickSlotNames[item.Index], BlueStonez.label_interparkbold_13pt_left);

				GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

				if ((Singleton<LoadoutManager>.Instance.Loadout.TryGetItem(loadoutSlotType, out var shopItem)) &&
				!Singleton<ItemManager>.Instance.IsDefaultGearItem(shopItem.View.PrefabName)) {
					GUILayout.Label($"{shopItem.View.Name} ({shopItem.View.ID})", BlueStonez.label_interparkbold_11pt_left);
					GUILayout.Space(ParadiseGUITools.LIST_ITEM_SPACING);

					if (GUILayout.Button(LocalizedStrings.Unequip, BlueStonez.buttondark_medium, GUILayout.Height(ParadiseGUITools.BUTTON_HEIGHT))) {
						Singleton<LoadoutManager>.Instance.ResetSlot(loadoutSlotType);
					}
				} else {
					GUI.enabled = false;
					GUILayout.Label("No item equipped", BlueStonez.label_interparkbold_11pt_left);
					GUI.enabled = true;
				}
			}
		}

		private void UpdateItemFilter() {
			switch (typeSelection.Current) {
				case UberstrikeItemType.Weapon:
					if (weaponClassSelection.Current == 0) {
						itemFilter = new ItemByTypeFilter(typeSelection.Current);
					} else {
						itemFilter = new ItemByClassFilter(typeSelection.Current, weaponClassSelection.Current);
					}
					break;
				case UberstrikeItemType.Gear:
					if (gearClassSelection.Current == 0) {
						itemFilter = new ItemByTypeFilter(typeSelection.Current);
					} else {
						itemFilter = new ItemByClassFilter(typeSelection.Current, gearClassSelection.Current);
					}
					break;
				case UberstrikeItemType.QuickUse:
				case UberstrikeItemType.Functional:
					itemFilter = new ItemByTypeFilter(typeSelection.Current);
					break;
			}

		}
	}
}
