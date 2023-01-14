using HarmonyLib;
using log4net;
using System.Reflection;
using UberStrike.Core.Models;
using UberStrike.Core.Types;

namespace Paradise.Client {
	public class HUDDesktopEventStreamHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Adds the name of the weapon a player has been killed with to the killfeed.
		/// </summary>
		public HUDDesktopEventStreamHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(HUDDesktopEventStreamHook)}] hooking {nameof(HUDDesktopEventStream)}");

			var orig_HUDDesktopEventStream_HandleKilledMessage = typeof(HUDDesktopEventStream).GetMethod("HandleKilledMessage", BindingFlags.Static | BindingFlags.Public);
			var prefix_HUDDesktopEventStream_HandleKilledMessage = typeof(HUDDesktopEventStreamHook).GetMethod("HandleKilledMessage_prefix", BindingFlags.Static | BindingFlags.Public);

			harmonyInstance.Patch(orig_HUDDesktopEventStream_HandleKilledMessage, new HarmonyMethod(prefix_HUDDesktopEventStream_HandleKilledMessage), null);
		}

		public static bool HandleKilledMessage_prefix(GameActorInfo shooter, GameActorInfo target, UberstrikeItemClass weapon, BodyPart bodyPart) {
			if (GameState.Current.GameMode == GameModeType.None) {
				return false;
			}

			if (target == null) {
				return false;
			}

			if (shooter == null || shooter == target) {
				GameData.Instance.OnHUDStreamMessage.Fire(target, LocalizedStrings.NKilledThemself, null);
			} else {
				var weaponName = Singleton<ItemManager>.Instance.GetItemInShop(shooter.CurrentWeaponID).Name;
				var killString = string.Empty;

				if (weapon == UberstrikeItemClass.WeaponMelee) {
					killString = $"[{weaponName}] smacked";
				} else if (bodyPart == BodyPart.Head) {
					killString = $"[{weaponName}] headshot";
				} else if (bodyPart == BodyPart.Nuts) {
					killString = $"[{weaponName}] nutshot";
				} else {
					killString = $"[{weaponName}] killed";
				}

				GameData.Instance.OnHUDStreamMessage.Fire(shooter, killString, target);
			}

			return false;
		}
	}
}
