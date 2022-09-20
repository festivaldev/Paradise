using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MemberWalletViewProxy {
		public static void Serialize(Stream stream, MemberWalletView instance) {
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.Cmid);
				Int32Proxy.Serialize(memoryStream, instance.Credits);
				DateTimeProxy.Serialize(memoryStream, instance.CreditsExpiration);
				Int32Proxy.Serialize(memoryStream, instance.Points);
				DateTimeProxy.Serialize(memoryStream, instance.PointsExpiration);
				memoryStream.WriteTo(stream);
			}
		}

		public static MemberWalletView Deserialize(Stream bytes) {
			return new MemberWalletView {
				Cmid = Int32Proxy.Deserialize(bytes),
				Credits = Int32Proxy.Deserialize(bytes),
				CreditsExpiration = DateTimeProxy.Deserialize(bytes),
				Points = Int32Proxy.Deserialize(bytes),
				PointsExpiration = DateTimeProxy.Deserialize(bytes)
			};
		}
	}
}
