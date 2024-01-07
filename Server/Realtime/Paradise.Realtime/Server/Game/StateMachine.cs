using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using EventHandler = Paradise.Realtime.Core.EventHandler;

namespace Paradise.Realtime.Server.Game {
	[JsonObject(MemberSerialization.OptIn)]
	public class StateMachine<T> where T : struct, IConvertible {
		private static readonly ILog Log = LogManager.GetLogger(nameof(StateMachine<T>));

		public event Action<T> OnChanged;
		public readonly EventHandler Events = new EventHandler();

		private readonly Dictionary<T, IState> registeredStates = new Dictionary<T, IState>();
		private readonly Stack<T> stateStack = new Stack<T>();

		private IState CurrentState {
			get {
				return GetState(CurrentStateId);
			}
		}

		[JsonProperty]
		public T CurrentStateId {
			get {
				return (stateStack.Count <= 0) ? default : stateStack.Peek();
			}
		}

		public bool ContainsState(T stateId) {
			return registeredStates.ContainsKey(stateId);
		}

		public IState GetState(T stateId) {
			registeredStates.TryGetValue(stateId, out var result);
			return result;
		}

		public void PopAllStates() {
			while (stateStack.Count > 0) {
				PopState(false);
			}
			OnChanged?.Invoke(default);
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
					OnChanged?.Invoke(stateId);
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
			OnChanged?.Invoke(default);
		}

		public void ResetState() {
			PopAllStates();
			stateStack.Clear();
			OnChanged?.Invoke(default);
		}

		public void SetState(T stateId) {
			if (ContainsState(stateId)) {
				if (!stateId.Equals(CurrentStateId)) {
					PopAllStates();
					stateStack.Push(stateId);
					GetState(stateId)?.OnEnter();
					OnChanged?.Invoke(stateId);
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
