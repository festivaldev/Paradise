using Cmune.DataCenter.Common.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UberStrike.Core.Types;
using UberStrike.DataCenter.Common.Entities;

namespace Paradise.WebServices.Services {
	internal class InventoryCommand : ParadiseCommand {
		public static new string Command => "inventory";
		public static new string[] Aliases => new string[] { "inv" };

		public override string Description => "Adds or removes items from a player's inventory.";
		public override string HelpString => $"{Command}\t{Description}";

		public override string[] UsageText => new string[] {
			$"{Command}: {Description}",
			"  give <name> <item>\t\tAdds the specified item to a player's inventory.",
			"  take <name> <item>\t\tRemoves the specified item from a player's inventory.",
			"  set <name> <slot> <item>\tSets the specified inventory slot to a specific item."
		};

		public override MemberAccessLevel MinimumAccessLevel => MemberAccessLevel.SeniorModerator;

		public InventoryCommand(Guid guid) : base(guid) { }

#pragma warning disable CS1998
		public override async Task Run(string[] arguments) {
			if (arguments.Length < 3) {
				PrintUsageText();
				return;
			}

			switch (arguments[0]) {
				case "give": {
					var searchString = arguments[1];

					if (searchString.Length < 3) {
						WriteLine("Search pattern must contain at least 3 characters.");
						return;
					}

					var publicProfile = DatabaseClient.GetProfile(searchString);

					if (publicProfile == null) {
						WriteLine($"Failed to add item to inventory: Could not find player matching {searchString}.");
						return;
					}

					if (!int.TryParse(arguments[2], out int itemId)) {
						WriteLine("Invalid parameter: item");
						return;
					}

					if (HasInventoryItem(publicProfile.Cmid, itemId)) {
						WriteLine("Failed to add item to inventory: Item is already in inventory.");
						return;
					}

					DatabaseClient.PlayerInventoryItems.Insert(new ItemInventoryView {
						Cmid = publicProfile.Cmid,
						ItemId = itemId,
						AmountRemaining = -1
					});

					WriteLine($"{(UberstrikeInventoryItem)itemId} added to player inventory.");

					break;
				}
				case "take": {
					var searchString = arguments[1];

					if (searchString.Length < 3) {
						WriteLine("Search pattern must contain at least 3 characters.");
						return;
					}

					var publicProfile = DatabaseClient.GetProfile(searchString);

					if (publicProfile == null) {
						WriteLine($"Failed to remove item from inventory: Could not find player matching {searchString}.");
						return;
					}

					if (!int.TryParse(arguments[2], out int itemId)) {
						WriteLine("Invalid parameter: item");
						return;
					}

					if (!HasInventoryItem(publicProfile.Cmid, itemId)) {
						WriteLine("Failed to remove item from inventory: Item is not in inventory.");
						return;
					}

					DatabaseClient.PlayerInventoryItems.DeleteMany(_ => _.Cmid == publicProfile.Cmid && _.ItemId == itemId);

					var playerLoadout = GetPlayerLoadout(publicProfile.Cmid);

					foreach (var loadoutSlot in Enum.GetValues(typeof(LoadoutSlotType))) {
						switch (loadoutSlot) {
							case LoadoutSlotType.Head:
								playerLoadout.Head = (playerLoadout.Head == itemId) ? 0 : playerLoadout.Head;
								break;
							case LoadoutSlotType.Gloves:
								playerLoadout.Gloves = (playerLoadout.Gloves == itemId) ? 0 : playerLoadout.Gloves;
								break;
							case LoadoutSlotType.UpperBody:
								playerLoadout.UpperBody = (playerLoadout.UpperBody == itemId) ? 0 : playerLoadout.UpperBody;
								break;
							case LoadoutSlotType.LowerBody:
								playerLoadout.LowerBody = (playerLoadout.LowerBody == itemId) ? 0 : playerLoadout.LowerBody;
								break;
							case LoadoutSlotType.Boots:
								playerLoadout.Boots = (playerLoadout.Boots == itemId) ? 0 : playerLoadout.Boots;
								break;
							case LoadoutSlotType.Face:
								playerLoadout.Face = (playerLoadout.Face == itemId) ? 0 : playerLoadout.Face;
								break;
							case LoadoutSlotType.Holo:
								playerLoadout.Webbing = (playerLoadout.Webbing == itemId) ? 0 : playerLoadout.Webbing;
								break;
							case LoadoutSlotType.MeleeWeapon:
								playerLoadout.MeleeWeapon = (playerLoadout.MeleeWeapon == itemId) ? 0 : playerLoadout.MeleeWeapon;
								break;
							case LoadoutSlotType.Weapon1:
								playerLoadout.Weapon1 = (playerLoadout.Weapon1 == itemId) ? 0 : playerLoadout.Weapon1;
								break;
							case LoadoutSlotType.Weapon2:
								playerLoadout.Weapon2 = (playerLoadout.Weapon2 == itemId) ? 0 : playerLoadout.Weapon2;
								break;
							case LoadoutSlotType.Weapon3:
								playerLoadout.Weapon3 = (playerLoadout.Weapon3 == itemId) ? 0 : playerLoadout.Weapon3;
								break;
							case LoadoutSlotType.QuickItem1:
								playerLoadout.QuickItem1 = (playerLoadout.QuickItem1 == itemId) ? 0 : playerLoadout.QuickItem1;
								break;
							case LoadoutSlotType.QuickItem2:
								playerLoadout.QuickItem2 = (playerLoadout.QuickItem2 == itemId) ? 0 : playerLoadout.QuickItem2;
								break;
							case LoadoutSlotType.QuickItem3:
								playerLoadout.QuickItem3 = (playerLoadout.QuickItem3 == itemId) ? 0 : playerLoadout.QuickItem3;
								break;
							case LoadoutSlotType.FunctionalItem1:
								playerLoadout.FunctionalItem1 = (playerLoadout.FunctionalItem1 == itemId) ? 0 : playerLoadout.FunctionalItem1;
								break;
							case LoadoutSlotType.FunctionalItem2:
								playerLoadout.FunctionalItem2 = (playerLoadout.FunctionalItem2 == itemId) ? 0 : playerLoadout.FunctionalItem2;
								break;
							case LoadoutSlotType.FunctionalItem3:
								playerLoadout.FunctionalItem3 = (playerLoadout.FunctionalItem3 == itemId) ? 0 : playerLoadout.FunctionalItem3;
								break;
							default:
								break;
						}
					}

					DatabaseClient.PlayerLoadouts.DeleteMany(_ => _.Cmid == playerLoadout.Cmid);
					DatabaseClient.PlayerLoadouts.Insert(playerLoadout);


					WriteLine($"{(UberstrikeInventoryItem)itemId} removed from player inventory.");

					break;
				}
				case "set": {
					if (arguments.Length < 4) {
						PrintUsageText();
						return;
					}

					var searchString = arguments[1];

					if (searchString.Length < 3) {
						WriteLine("Search pattern must contain at least 3 characters.");
						return;
					}

					var publicProfile = DatabaseClient.GetProfile(searchString);

					if (publicProfile == null) {
						WriteLine($"Failed to set loadout slot: Could not find player matching {searchString}.");
						return;
					}

					var forbiddenSlots = new List<string> {
						"LoadoutID",
						"Backpack",
						"Cmid",
						"Type",
						"Weapon1Mod1",
						"Weapon1Mod2",
						"Weapon1Mod3",
						"Weapon2Mod1",
						"Weapon2Mod2",
						"Weapon2Mod3",
						"Weapon3Mod1",
						"Weapon3Mod2",
						"Weapon3Mod3",
					};

					if (forbiddenSlots.Contains(arguments[2]) || !(typeof(LoadoutView).GetProperty(arguments[2], BindingFlags.Public | BindingFlags.Instance) is PropertyInfo slotProperty)) {
						WriteLine("Invalid parameter: slot");
						return;
					}

					if (!int.TryParse(arguments[3], out int itemId)) {
						WriteLine("Invalid parameter: item");
						return;
					}

					if (itemId > 0 && !HasInventoryItem(publicProfile.Cmid, itemId)) {
						WriteLine("Failed to set loadout slot: Item is not in inventory.");
						return;
					}

					var playerLoadout = GetPlayerLoadout(publicProfile.Cmid);

					slotProperty.SetValue(playerLoadout, itemId);

					DatabaseClient.PlayerLoadouts.DeleteMany(_ => _.Cmid == playerLoadout.Cmid);
					DatabaseClient.PlayerLoadouts.Insert(playerLoadout);

					WriteLine($"Slot {arguments[2]} has been set to {(UberstrikeInventoryItem)itemId}.");

					break;
				}
				default:
					WriteLine($"{Command}: unknown command {arguments[0]}\n");
					break;
			}
		}
#pragma warning restore CS1998

		private bool HasInventoryItem(int cmid, int itemId) {
			return DatabaseClient.PlayerInventoryItems.FindOne(_ => _.Cmid == cmid && _.ItemId == itemId && (_.ExpirationDate > DateTime.UtcNow || _.ExpirationDate == null)) != null;
		}

		private LoadoutView GetPlayerLoadout(int cmid) {
			return DatabaseClient.PlayerLoadouts.FindOne(_ => _.Cmid == cmid);
		}
	}
}
