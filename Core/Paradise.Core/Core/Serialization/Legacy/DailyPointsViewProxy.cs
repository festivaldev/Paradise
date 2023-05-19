using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class DailyPointsViewProxy {
		public static void Serialize(Stream stream, DailyPointsView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.Current);
					Int32Proxy.Serialize(memoryStream, instance.PointsMax);
					Int32Proxy.Serialize(memoryStream, instance.PointsTomorrow);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static DailyPointsView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			DailyPointsView dailyPointsView = null;
			if (num != 0) {
				dailyPointsView = new DailyPointsView();
				dailyPointsView.Current = Int32Proxy.Deserialize(bytes);
				dailyPointsView.PointsMax = Int32Proxy.Deserialize(bytes);
				dailyPointsView.PointsTomorrow = Int32Proxy.Deserialize(bytes);
			}
			return dailyPointsView;
		}
	}
}
