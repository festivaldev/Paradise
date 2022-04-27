using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client.Bootstrap {
	public static class Bootstrapper {
		public static void Initialize() {
			var hooks = new List<IParadiseHook> {
				// Adds update logic on game start
				new MenuPageManagerHook(),

				// Redirects web services to configured URLs and allows loading custom maps
				new ApplicationDataManagerHook(),

				// Adds missing screen resolutions to settings pane
				new ScreenResolutionManagerHook(),

				// Redirects bundle purchases to our web services
				new BundleManagerHook(),

				// Brings back Quick Switching
				new WeaponControllerHook(),

				// Reimplements the game creation GUI to allow selecting mods
				new CreateGamePanelGUIHook()
			};

			foreach (var hook in hooks) {
				Debug.Log($"[Paradise] Hooking {hook.TypeToHook.Name} using {hook.GetType().Name}");
				hook.Hook();
			}
		}
	}
}
