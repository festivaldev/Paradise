using HarmonyLib;
using System;

namespace Paradise.Client {
	public class ParadiseTraverse {
		public object Instance;

		private readonly Traverse traverse;


		public static ParadiseTraverse Create(Type traverseType) {
			return new ParadiseTraverse(traverseType);
		}

		public static ParadiseTraverse Create(object instance) {
			return new ParadiseTraverse(instance);
		}

		private ParadiseTraverse(Type traverseType) {
			traverse = Traverse.Create(traverseType);
		}

		private ParadiseTraverse(object instance) {
			traverse = Traverse.Create(instance);
			Instance = instance;
		}


		public T GetField<T>(string fieldName) {
			return traverse.Field<T>(fieldName).Value;
		}

		public void SetField(string fieldName, object value) {
			traverse.Field(fieldName).SetValue(value);
		}

		public T GetProperty<T>(string propertyName) {
			return traverse.Property<T>(propertyName).Value;
		}

		public void SetProperty(string propertyName, object value) {
			traverse.Property(propertyName).SetValue(value);
		}

		public object InvokeMethod(string methodName, params object[] parameters) {
			return InvokeMethod(Instance, methodName, parameters);
		}

		public object InvokeMethod(object instance, string methodName, params object[] parameters) {
			if (instance == null) throw new NullReferenceException("Instance cannot be null!");

			return AccessTools.Method(instance.GetType(), methodName).Invoke(instance, parameters);
		}
	}
}
