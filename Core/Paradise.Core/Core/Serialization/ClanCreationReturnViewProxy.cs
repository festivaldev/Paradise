using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ClanCreationReturnViewProxy {
		public static void Serialize(Stream stream, ClanCreationReturnView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.ClanView != null) {
					ClanViewProxy.Serialize(memoryStream, instance.ClanView);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.ResultCode);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static ClanCreationReturnView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ClanCreationReturnView clanCreationReturnView = new ClanCreationReturnView();
			if ((num & 1) != 0) {
				clanCreationReturnView.ClanView = ClanViewProxy.Deserialize(bytes);
			}
			clanCreationReturnView.ResultCode = Int32Proxy.Deserialize(bytes);
			return clanCreationReturnView;
		}
	}
}
