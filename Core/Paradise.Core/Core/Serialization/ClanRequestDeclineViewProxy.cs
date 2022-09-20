using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ClanRequestDeclineViewProxy {
		public static void Serialize(Stream stream, ClanRequestDeclineView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.ActionResult);
				Int32Proxy.Serialize(memoryStream, instance.ClanRequestId);
				memoryStream.WriteTo(stream);
			}
		}

		public static ClanRequestDeclineView Deserialize(Stream bytes) {
			return new ClanRequestDeclineView {
				ActionResult = Int32Proxy.Deserialize(bytes),
				ClanRequestId = Int32Proxy.Deserialize(bytes)
			};
		}
	}
}
