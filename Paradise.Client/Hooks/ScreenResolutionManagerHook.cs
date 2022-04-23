using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Client {
	public class ScreenResolutionManagerHook : IParadiseHook {
		public Type TypeToHook => typeof(ScreenResolutionManager);

		public static void AddResolutions(List<Resolution> resolutions) {
			if (resolutions.Count == 0 || !resolutions.Contains(Screen.currentResolution)) {
				resolutions.Add(Screen.currentResolution);
			}
		}

		public void Hook() {
			if (ScreenResolutionManager.Resolutions.Count == 0 || !ScreenResolutionManager.Resolutions.Contains(Screen.currentResolution)) {
				ScreenResolutionManager.Resolutions.Add(Screen.currentResolution);
			}
		}
	}
}
