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
			return traverse.Method(methodName, parameters).GetValue();
		}


		public static T GetField<T>(object instance, string fieldName) {
			if (instance == null) throw new NullReferenceException("Instance cannot be null!");

			return (T)AccessTools.Field(instance.GetType(), fieldName).GetValue(instance);
		}

		public static void SetField(object instance, string fieldName, object value) {
			if (instance == null) throw new NullReferenceException("Instance cannot be null!");

			AccessTools.Field(instance.GetType(), fieldName).SetValue(instance, value);
		}

		public static T GetProperty<T>(object instance, string propertyName) {
			if (instance == null) throw new NullReferenceException("Instance cannot be null!");

			return (T)AccessTools.Property(instance.GetType(), propertyName).GetValue(instance, null);
		}

		public static void SetProperty(object instance, string propertyName, object value) {
			if (instance == null) throw new NullReferenceException("Instance cannot be null!");

			AccessTools.Property(instance.GetType(), propertyName).SetValue(instance, value, null);
		}

		public static object InvokeMethod(object instance, string methodName, params object[] parameters) {
			if (instance == null) throw new NullReferenceException("Instance cannot be null!");

			return AccessTools.Method(instance.GetType(), methodName).Invoke(instance, parameters);
		}
	}
}
