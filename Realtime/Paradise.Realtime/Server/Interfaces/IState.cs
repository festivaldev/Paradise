namespace Paradise.Realtime.Server {
	public interface IState {
		void OnEnter();
		void OnExit();
		void OnResume();
		void OnUpdate();
	}
}
