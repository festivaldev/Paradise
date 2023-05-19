using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class MemberWalletViewProxy {
		public static void Serialize(Stream stream, MemberWalletView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					Int32Proxy.Serialize(memoryStream, instance.Credits);
					DateTimeProxy.Serialize(memoryStream, instance.CreditsExpiration);
					Int32Proxy.Serialize(memoryStream, instance.Points);
					DateTimeProxy.Serialize(memoryStream, instance.PointsExpiration);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static MemberWalletView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MemberWalletView memberWalletView = null;
			if (num != 0) {
				memberWalletView = new MemberWalletView();
				memberWalletView.Cmid = Int32Proxy.Deserialize(bytes);
				memberWalletView.Credits = Int32Proxy.Deserialize(bytes);
				memberWalletView.CreditsExpiration = DateTimeProxy.Deserialize(bytes);
				memberWalletView.Points = Int32Proxy.Deserialize(bytes);
				memberWalletView.PointsExpiration = DateTimeProxy.Deserialize(bytes);
			}
			return memberWalletView;
		}
	}
}
