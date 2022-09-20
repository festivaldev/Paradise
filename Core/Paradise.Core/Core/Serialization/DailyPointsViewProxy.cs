using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class DailyPointsViewProxy {
		public static void Serialize(Stream stream, DailyPointsView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.Current);
				Int32Proxy.Serialize(memoryStream, instance.PointsMax);
				Int32Proxy.Serialize(memoryStream, instance.PointsTomorrow);
				memoryStream.WriteTo(stream);
			}
		}

		public static DailyPointsView Deserialize(Stream bytes) {
			return new DailyPointsView {
				Current = Int32Proxy.Deserialize(bytes),
				PointsMax = Int32Proxy.Deserialize(bytes),
				PointsTomorrow = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
