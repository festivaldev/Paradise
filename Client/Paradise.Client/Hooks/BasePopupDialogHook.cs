using HarmonyLib;

namespace Paradise.Client {
	/// <summary>
	/// Allows displaying popups above panels
	/// </summary>
	[HarmonyPatch(typeof(BasePopupDialog))]
	public class BasePopupDialogHook {
		[HarmonyPatch("get_Depth"), HarmonyPostfix]
		public static void get_Depth_Postfix(ref GuiDepth __result) {
			__result = (GuiDepth)(-100);
		}
	}
}
