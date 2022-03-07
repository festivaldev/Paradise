using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client {
	public class ScreenResolutionManager_hook {
		public static void AddResolutions(List<Resolution> resolutions) {
			if (resolutions.Count == 0 || !resolutions.Contains(Screen.currentResolution)) {
				resolutions.Add(Screen.currentResolution);
			}
		}
	}
}
