using HarmonyLib;
using System.Collections.Generic;

namespace Paradise.Client.Bootstrap {
	public static class Bootstrapper {
		public static void Initialize() {
			ParadiseClient.Initialize();

			var harmonyInstance = new Harmony("tf.festival.Paradise");

			var hooks = new List<IParadiseHook> {
#if DEBUG
				new ClanDataManagerHook(),
				new TrainingRoomHook(),
#endif

				new AuthenticationManagerHook(),
				new ApplicationDataManagerHook(),
				new SoapClientHook(),
				new MenuPageManagerHook(),
				new MapManagerHook(),
				new ScreenResolutionManagerHook(),
				new OptionsPanelGUIHook(),
				new BundleManagerHook(),
				new WeaponControllerHook(),
				new CreateGamePanelGUIHook(),
				new HUDStatusPanelHook(),
				new PlayerKilledSpectatorStateHook(),
				new SfxManagerHook(),
				new GameStateHook(),
				new PlayerLeadAudioHook()
			};

			foreach (var hook in hooks) {
				hook.Hook(harmonyInstance);
			}
		}
	}
}
