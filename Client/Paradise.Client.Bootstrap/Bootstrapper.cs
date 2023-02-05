using HarmonyLib;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Reflection;

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
				new GlobalSceneLoaderHook(),
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
				new HUDDesktopEventStreamHook(),
				new PlayerKilledSpectatorStateHook(),
				new SfxManagerHook(),
				new GameStateHook(),
				new PlayerLeadAudioHook(),
				new GlobalUIRibbonHook()
			};

			foreach (var hook in hooks) {
				hook.Hook(harmonyInstance);
			}

			// Register URI handler
			if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsPlayer) {
				if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\uberstrike") == null) {
					using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\uberstrike")) {
						var appLocation = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

						key.SetValue("", "URL:UberStrike");
						key.SetValue("URL Protocol", "");

						using (var defaultIcon = key.CreateSubKey("DefaultIcon")) {
							defaultIcon.SetValue("", appLocation + ",1");
						}

						using (var commandKey = key.CreateSubKey(@"shell\open\command")) {
							commandKey.SetValue("", "\"" + appLocation + "\" \"%1\"");
						}
					}
				}
			}
		}
	}
}
