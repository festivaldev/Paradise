namespace Paradise {
	public interface ILoop {
		float Time { get; }
		float DeltaTime { get; }

		void Setup();
		void Tick();
		void Teardown();
	}
}
