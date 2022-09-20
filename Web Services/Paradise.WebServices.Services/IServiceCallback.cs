namespace Paradise.WebServices {
	public interface IServiceCallback {
		void OnServiceStarted(object sender, ServiceEventArgs args);
		void OnServiceStopped(object sender, ServiceEventArgs args);
		void OnServiceError(object sender, ServiceEventArgs args);
	}
}
