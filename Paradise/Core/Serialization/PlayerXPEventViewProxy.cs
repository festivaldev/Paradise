﻿using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class PlayerXPEventViewProxy {
		public static void Serialize(Stream stream, PlayerXPEventView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.Name != null) {
					StringProxy.Serialize(memoryStream, instance.Name);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.PlayerXPEventId);
				DecimalProxy.Serialize(memoryStream, instance.XPMultiplier);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static PlayerXPEventView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PlayerXPEventView playerXPEventView = new PlayerXPEventView();
			if ((num & 1) != 0) {
				playerXPEventView.Name = StringProxy.Deserialize(bytes);
			}
			playerXPEventView.PlayerXPEventId = Int32Proxy.Deserialize(bytes);
			playerXPEventView.XPMultiplier = DecimalProxy.Deserialize(bytes);
			return playerXPEventView;
		}
	}
}
