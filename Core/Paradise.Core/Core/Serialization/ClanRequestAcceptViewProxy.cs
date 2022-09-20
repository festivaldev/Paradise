using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ClanRequestAcceptViewProxy {
		public static void Serialize(Stream stream, ClanRequestAcceptView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.ActionResult);
				Int32Proxy.Serialize(memoryStream, instance.ClanRequestId);
				if (instance.ClanView != null) {
					ClanViewProxy.Serialize(memoryStream, instance.ClanView);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static ClanRequestAcceptView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ClanRequestAcceptView clanRequestAcceptView = new ClanRequestAcceptView();
			clanRequestAcceptView.ActionResult = Int32Proxy.Deserialize(bytes);
			clanRequestAcceptView.ClanRequestId = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				clanRequestAcceptView.ClanView = ClanViewProxy.Deserialize(bytes);
			}
			return clanRequestAcceptView;
		}
	}
}
