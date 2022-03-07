using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MemberPositionUpdateViewProxy {
		public static void Serialize(Stream stream, MemberPositionUpdateView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				if (instance.AuthToken != null) {
					StringProxy.Serialize(memoryStream, instance.AuthToken);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.GroupId);
				Int32Proxy.Serialize(memoryStream, instance.MemberCmid);
				EnumProxy<GroupPosition>.Serialize(memoryStream, instance.Position);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static MemberPositionUpdateView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MemberPositionUpdateView memberPositionUpdateView = new MemberPositionUpdateView();
			if ((num & 1) != 0) {
				memberPositionUpdateView.AuthToken = StringProxy.Deserialize(bytes);
			}
			memberPositionUpdateView.GroupId = Int32Proxy.Deserialize(bytes);
			memberPositionUpdateView.MemberCmid = Int32Proxy.Deserialize(bytes);
			memberPositionUpdateView.Position = EnumProxy<GroupPosition>.Deserialize(bytes);
			return memberPositionUpdateView;
		}
	}
}
