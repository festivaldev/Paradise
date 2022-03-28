using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client {
	public class ScreenResolutionManager_hook {
		public static void AddResolutions(List<Resolution> resolutions) {
			if (resolutions.Count == 0 || !resolutions.Contains(Screen.currentResolution)) {
				resolutions.Add(Screen.currentResolution);
			}
		}

		public static void Hook() {
			if (ScreenResolutionManager.Resolutions.Count == 0 || !ScreenResolutionManager.Resolutions.Contains(Screen.currentResolution)) {
				ScreenResolutionManager.Resolutions.Add(Screen.currentResolution);
			}
		}
	}
}
