using Paradise.Core.Models.Views;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ParadiseMapViewProxy {
		public static void Serialize(Stream stream, ParadiseMapView instance) {
			using (var memoryStream = new MemoryStream()) {
				MapViewProxy.Serialize(memoryStream, instance);
				StringProxy.Serialize(memoryStream, instance.FileName);
				memoryStream.WriteTo(stream);
			}
		}

		public static ParadiseMapView Deserialize(Stream bytes) {
			var mapView = (ParadiseMapView)MapViewProxy.Deserialize(bytes);
			mapView.FileName = StringProxy.Deserialize(bytes);

			return mapView;
		}
	}
}
