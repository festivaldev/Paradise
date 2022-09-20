using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Paradise.Core.Models {
	public static class StatsCollectionHelper {
		static StatsCollectionHelper() {
			PropertyInfo[] array = typeof(StatsCollection).GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in array) {
				if (propertyInfo.PropertyType == typeof(int) && propertyInfo.CanRead && propertyInfo.CanWrite) {
					StatsCollectionHelper.properties.Add(propertyInfo);
				}
			}
		}

		public static string ToString(StatsCollection instance) {
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PropertyInfo propertyInfo in StatsCollectionHelper.properties) {
				stringBuilder.AppendFormat("{0}:{1}\n", propertyInfo.Name, propertyInfo.GetValue(instance, null));
			}
			return stringBuilder.ToString();
		}

		public static void Reset(StatsCollection instance) {
			foreach (PropertyInfo propertyInfo in StatsCollectionHelper.properties) {
				propertyInfo.SetValue(instance, 0, null);
			}
		}

		public static void TakeBestValues(StatsCollection instance, StatsCollection that) {
			foreach (PropertyInfo propertyInfo in StatsCollectionHelper.properties) {
				int num = (int)propertyInfo.GetValue(instance, null);
				int num2 = (int)propertyInfo.GetValue(that, null);
				if (num < num2) {
					propertyInfo.SetValue(instance, num2, null);
				}
			}
		}

		public static void AddAllValues(StatsCollection instance, StatsCollection that) {
			foreach (PropertyInfo propertyInfo in StatsCollectionHelper.properties) {
				int num = (int)propertyInfo.GetValue(instance, null);
				int num2 = (int)propertyInfo.GetValue(that, null);
				propertyInfo.SetValue(instance, num + num2, null);
			}
		}

		private static List<PropertyInfo> properties = new List<PropertyInfo>();
	}
}
