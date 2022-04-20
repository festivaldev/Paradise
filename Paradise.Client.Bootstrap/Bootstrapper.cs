namespace Paradise.Client.Bootstrap {
	public static class Bootstrapper {
		public static void Initialize() {
			MenuPageManager_hook.Hook();
			ApplicationDataManager_hook.Hook();
			ScreenResolutionManager_hook.Hook();
			BundleManager_hook.Hook();
			WeaponController_hook.Hook();
		}
	}
}
