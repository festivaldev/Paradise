using log4net;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server {
	public class StateMachine<T> where T : struct, IConvertible {
		private static readonly ILog Log = LogManager.GetLogger(nameof(StateMachine<T>));

		public event Action<T> OnChanged;
		public readonly EventHandler Events = new EventHandler();

		private Dictionary<T, IState> registeredStates = new Dictionary<T, IState>();
		private Stack<T> stateStack = new Stack<T>();

		private IState CurrentState {
			get {
				return GetState(CurrentStateId);
			}
		}

		public T CurrentStateId {
			get {
				return (stateStack.Count <= 0) ? default(T) : stateStack.Peek();
			}
		}

		public bool ContainsState(T stateId) {
			return registeredStates.ContainsKey(stateId);
		}

		public IState GetState(T stateId) {
			IState result;
			registeredStates.TryGetValue(stateId, out result);
			return result;
		}

		public void PopAllStates() {
			while (stateStack.Count > 0) {
				PopState(false);
			}
			if (OnChanged != null) {
				OnChanged(default(T));
			}
		}

		public void PopState(bool resume = false) {
			if (stateStack.Count != 0) {
				CurrentState?.OnExit();
				stateStack.Pop();
				if (resume && stateStack.Count != 0) {
					CurrentState.OnResume();
				}
				if (OnChanged != null && stateStack.Count > 0) {
					OnChanged(stateStack.Peek());
				}
			}
		}

		public void PushState(T stateId) {
			if (ContainsState(stateId)) {
				if (!stateStack.Contains(stateId)) {
					stateStack.Push(stateId);
					GetState(stateId).OnEnter();
					if (OnChanged != null) {
						OnChanged(stateId);
					}
				}
			} else {
				Console.WriteLine("Unsupported state of type: " + stateId);
			}
		}

		public void RegisterState(T stateId, IState state) {
			if (!registeredStates.ContainsKey(stateId)) {
				registeredStates.Add(stateId, state);
				return;
			}
			throw new Exception("StateMachine::RegisterState - state [" + stateId + "] already exists in the current registry");
		}

		public void Reset() {
			PopAllStates();
			stateStack.Clear();
			registeredStates.Clear();
			Events.Clear();
			if (OnChanged != null) {
				OnChanged(default(T));
			}
		}

		public void ResetState() {
			PopAllStates();
			stateStack.Clear();
			if (OnChanged != null) {
				OnChanged(default(T));
			}
		}

		public void SetState(T stateId) {
			if (ContainsState(stateId)) {
				if (!stateId.Equals(CurrentStateId)) {
					PopAllStates();
					stateStack.Push(stateId);
					GetState(stateId)?.OnEnter();
					if (OnChanged != null) {
						OnChanged(stateId);
					}
				}
				return;
			}
			throw new Exception("Unsupported state of type: " + stateId);
		}

		public void Update() {
			CurrentState?.OnUpdate();
		}
	}
}
