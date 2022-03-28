using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Paradise.Client;

namespace Paradise.Client.Bootstrap {
	public static class Bootstrapper {
		public static void Initialize() {
			ApplicationDataManager_hook.Hook();
			ScreenResolutionManager_hook.Hook();
			BundleManager_hook.Hook();
		}
	}
}
