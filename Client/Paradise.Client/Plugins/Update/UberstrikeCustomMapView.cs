﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UberStrike.Core.Serialization;

namespace Paradise.Client {
	[Serializable]
	public class UberstrikeCustomMapView {
		public string Name { get; set; }
		public string FileName { get; set; }
		public int MapId { get; set; }
		public List<int> SupportedGameModes;

		public override string ToString() {
			return $"UberstrikeCustomMapView<Name=\"{Name}\" FileName=\"{FileName}\" MapId={MapId} SupportedGameModes=[{string.Join(",", SupportedGameModes.Select(_ => _.ToString()).ToArray())}]>";
		}
	}

	public static class UberstrikeCustomMapViewProxy {
		public static void Serialize(Stream stream, UberstrikeCustomMapView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				StringProxy.Serialize(memoryStream, instance.Name);
				StringProxy.Serialize(memoryStream, instance.FileName);
				Int32Proxy.Serialize(memoryStream, instance.MapId);
				ListProxy<int>.Serialize(memoryStream, instance.SupportedGameModes, Int32Proxy.Serialize);

				memoryStream.WriteTo(stream);
			}
		}

		public static UberstrikeCustomMapView Deserialize(Stream bytes) {
			UberstrikeCustomMapView mapView = new UberstrikeCustomMapView();

			mapView.Name = StringProxy.Deserialize(bytes);
			mapView.FileName = StringProxy.Deserialize(bytes);
			mapView.MapId = Int32Proxy.Deserialize(bytes);
			mapView.SupportedGameModes = ListProxy<int>.Deserialize(bytes, Int32Proxy.Deserialize);

			return mapView;
		}
	}
}