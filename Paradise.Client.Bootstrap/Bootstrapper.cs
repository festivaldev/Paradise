namespace Paradise.Client.Bootstrap {
	public static class Bootstrapper {
		public static void Initialize() {
			// Adds update logic on game start
			MenuPageManager_hook.Hook();

			// Redirects web services to configured URLs and allows loading custom maps
			ApplicationDataManager_hook.Hook();

			// Adds missing screen resolutions to settings pane
			ScreenResolutionManager_hook.Hook();

			// Redirects bundle purchases to our web services
			BundleManager_hook.Hook();

			// Brings back Quick Switching
			WeaponController_hook.Hook();
		}
	}
}
