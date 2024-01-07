namespace Paradise.Realtime.Core {
	public interface ILoop {
		float Time { get; }
		float DeltaTime { get; }
		void Setup();
		void Tick();
		void Teardown();
	}
}
