using System;
using System.Collections.Generic;

public class EventHandler {
	public void AddListener<T>(Action<T> callback) {
		global::EventHandler.IEventContainer eventContainer;
		if (!this.eventContainer.TryGetValue(typeof(T), out eventContainer)) {
			eventContainer = new global::EventHandler.EventContainer<T>();
			this.eventContainer.Add(typeof(T), eventContainer);
		}
		global::EventHandler.EventContainer<T> eventContainer2 = eventContainer as global::EventHandler.EventContainer<T>;
		if (eventContainer2 != null) {
			eventContainer2.AddCallbackMethod(callback);
		}
	}

	public void Clear() {
		this.eventContainer.Clear();
	}

	public void Fire(object message) {
		global::EventHandler.IEventContainer eventContainer;
		if (this.eventContainer.TryGetValue(message.GetType(), out eventContainer)) {
			eventContainer.CastEvent(message);
		}
	}

	public void RemoveListener<T>(Action<T> callback) {
		global::EventHandler.IEventContainer eventContainer;
		if (this.eventContainer.TryGetValue(typeof(T), out eventContainer)) {
			global::EventHandler.EventContainer<T> eventContainer2 = eventContainer as global::EventHandler.EventContainer<T>;
			if (eventContainer2 != null) {
				eventContainer2.RemoveCallbackMethod(callback);
			}
		}
	}

	private Dictionary<Type, global::EventHandler.IEventContainer> eventContainer = new Dictionary<Type, global::EventHandler.IEventContainer>();

	public static readonly global::EventHandler Global = new global::EventHandler();

	private class EventContainer<T> : global::EventHandler.IEventContainer {
		public void AddCallbackMethod(Action<T> callback) {
			string callbackMethodId = this.GetCallbackMethodId(callback);
			if (!this._dictionary.ContainsKey(callbackMethodId)) {
				this._dictionary.Add(callbackMethodId, callback);
				this.Sender = (Action<T>)Delegate.Combine(this.Sender, callback);
			}
		}

		public void CastEvent(object m) {
			if (this.Sender != null) {
				this.Sender((T)((object)m));
			}
		}

		private string GetCallbackMethodId(Action<T> callback) {
			string text = callback.Method.DeclaringType.FullName + callback.Method.Name;
			if (callback.Target != null) {
				text += callback.Target.GetHashCode().ToString();
			}
			return text;
		}

		public void RemoveCallbackMethod(Action<T> callback) {
			if (this._dictionary.Remove(this.GetCallbackMethodId(callback))) {
				this.Sender = (Action<T>)Delegate.Remove(this.Sender, callback);
			}
		}

		public event Action<T> Sender;

		private Dictionary<string, Action<T>> _dictionary = new Dictionary<string, Action<T>>();
	}

	private interface IEventContainer {
		void CastEvent(object m);
	}
}
