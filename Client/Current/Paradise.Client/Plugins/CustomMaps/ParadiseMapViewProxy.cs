using System.IO;
using UberStrike.Core.Models.Views;

namespace UberStrike.Core.Serialization {
	public static class ParadiseMapViewProxy {
		public static void Serialize(Stream stream, ParadiseMapView instance) {
			using (var memoryStream = new MemoryStream()) {
				MapViewProxy.Serialize(memoryStream, instance);
				StringProxy.Serialize(memoryStream, instance.FileName);
				memoryStream.WriteTo(stream);
			}
		}

		public static ParadiseMapView Deserialize(Stream bytes) {
			var mapView = MapViewProxy.Deserialize(bytes);

			return new ParadiseMapView(mapView) {
				FileName = StringProxy.Deserialize(bytes)
			};
		}
	}
}
