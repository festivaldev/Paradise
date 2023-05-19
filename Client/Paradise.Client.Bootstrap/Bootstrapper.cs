using HarmonyLib;
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Paradise.Client.Bootstrap {
	public static class Bootstrapper {
		public static void Initialize() {
			// Fix the current working directory if launched via URI protocol
			Environment.CurrentDirectory = Path.GetDirectoryName(Application.dataPath);

			ParadiseClient.Initialize();

			var harmonyInstance = new Harmony("tf.festival.Paradise");
			harmonyInstance.PatchAll(Assembly.GetAssembly(typeof(ParadiseClient)));

			// Register URI handler
			if (Application.platform == RuntimePlatform.WindowsPlayer) {
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
