using System.Reflection;

namespace Paradise.Client {
	public interface IParadiseHook {
		void Hook(HarmonyLib.Harmony harmonyInstance);
	}

	public abstract class ParadiseHook : IParadiseHook {
		public abstract void Hook(HarmonyLib.Harmony harmonyInstance);


		public static T GetField<T>(object instance, string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			return (T)instance.GetType().GetField(fieldName, flags).GetValue(instance);
		}

		public static void SetField(object instance, string fieldName, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			instance.GetType().GetField(fieldName, flags).SetValue(instance, value);
		}

		public static T GetProperty<T>(object instance, string propertyName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			return (T)instance.GetType().GetProperty(propertyName, flags).GetValue(instance, null);
		}

		public static void SetProperty(object instance, string propertyName, object value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			instance.GetType().GetProperty(propertyName, flags).SetValue(instance, value, null);
		}

		public static object InvokeMethod(object instance, string methodName, object[] parameters, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic) {
			return instance.GetType().GetMethod(methodName, flags).Invoke(instance, parameters);
		}
	}
}
