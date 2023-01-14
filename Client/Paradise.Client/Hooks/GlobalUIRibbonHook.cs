using HarmonyLib;
using log4net;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client {
	public class GlobalUIRibbonHook : ParadiseHook {
		private static readonly ILog Log = LogManager.GetLogger(nameof(IParadiseHook));

		/// <summary>
		/// Adds additional entries to the global ribbon menu
		/// </summary>
		public GlobalUIRibbonHook() { }

		public override void Hook(Harmony harmonyInstance) {
			Log.Info($"[{nameof(GlobalUIRibbonHook)}] hooking {nameof(GlobalUIRibbon)}");

			var orig_GlobalUIRibbon_InitOptionsDropdown = typeof(GlobalUIRibbon).GetMethod("InitOptionsDropdown", BindingFlags.NonPublic | BindingFlags.Instance);
			var postfix_GlobalUIRibbon_InitOptionsDropdown = typeof(GlobalUIRibbonHook).GetMethod("InitOptionsDropdown_Postfix", BindingFlags.Public | BindingFlags.Static);

			harmonyInstance.Patch(orig_GlobalUIRibbon_InitOptionsDropdown, null, new HarmonyMethod(postfix_GlobalUIRibbon_InitOptionsDropdown));
		}

		public static void InitOptionsDropdown_Postfix(GlobalUIRibbon __instance) {
			var optionsDropdown = GetField<GuiDropDown>(__instance, "_optionsDropdown");

			var holder = AutoMonoBehaviour<TextureLoader>.Instance.Load(ApplicationDataManager.ImagePath + "github.png", null);

			optionsDropdown.Add(new GUIContent(" Report Issue", holder.Texture), delegate () {
				Application.OpenURL("https://github.com/festivaldev/Paradise/issues");
			});
		}
	}
}
