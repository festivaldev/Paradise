using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client.Bootstrap {
	public static class Bootstrapper {
		public static void Initialize() {
			var harmony = new Harmony("tf.festival.Paradise");

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
				new CreateGamePanelGUIHook(),

				// Add debug camera for internal staff (Ctrl-Alt-C)
				new TrainingRoomHook(),

				// Change "Kills remaining" string to "Rounds remaining" in Team Elimination
				// and add appropriate game mode hint
				new HUDStatusPanelHook(),

				// Team Elimination: After a player is killed, move them to spectator state
				new PlayerKilledSpectatorStateHook()
			};

			foreach (var hook in hooks) {
				hook.Hook(harmony);
			}
		}
	}
}
