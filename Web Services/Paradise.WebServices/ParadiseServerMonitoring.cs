using System.Collections.Generic;

namespace Paradise.WebServices {
	public class ParadiseServerMonitoring {
		private static Dictionary<string, object> _commMonitoringData = new Dictionary<string, object>();
		private static Dictionary<string, object> _gameMonitoringData = new Dictionary<string, object>();

		public static IReadOnlyDictionary<string, object> CommMonitoringData => _commMonitoringData;
		public static IReadOnlyDictionary<string, object> GameMonitoringData => _gameMonitoringData;

		internal static void SetCommMonitoringData(Dictionary<string, object> data) {
			_commMonitoringData = data;
		}

		internal static void SetGameMonitoringData(string identifier, Dictionary<string, object> data) {
			_gameMonitoringData[identifier] = data;
		}
	}
}
