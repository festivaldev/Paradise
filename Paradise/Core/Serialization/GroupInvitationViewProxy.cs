using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class GroupInvitationViewProxy {
		public static void Serialize(Stream stream, GroupInvitationView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				Int32Proxy.Serialize(memoryStream, instance.GroupId);
				Int32Proxy.Serialize(memoryStream, instance.GroupInvitationId);
				if (instance.GroupName != null) {
					StringProxy.Serialize(memoryStream, instance.GroupName);
				} else {
					num |= 1;
				}
				if (instance.GroupTag != null) {
					StringProxy.Serialize(memoryStream, instance.GroupTag);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(memoryStream, instance.InviteeCmid);
				if (instance.InviteeName != null) {
					StringProxy.Serialize(memoryStream, instance.InviteeName);
				} else {
					num |= 4;
				}
				Int32Proxy.Serialize(memoryStream, instance.InviterCmid);
				if (instance.InviterName != null) {
					StringProxy.Serialize(memoryStream, instance.InviterName);
				} else {
					num |= 8;
				}
				if (instance.Message != null) {
					StringProxy.Serialize(memoryStream, instance.Message);
				} else {
					num |= 16;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static GroupInvitationView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			GroupInvitationView groupInvitationView = new GroupInvitationView();
			groupInvitationView.GroupId = Int32Proxy.Deserialize(bytes);
			groupInvitationView.GroupInvitationId = Int32Proxy.Deserialize(bytes);
			if ((num & 1) != 0) {
				groupInvitationView.GroupName = StringProxy.Deserialize(bytes);
			}
			if ((num & 2) != 0) {
				groupInvitationView.GroupTag = StringProxy.Deserialize(bytes);
			}
			groupInvitationView.InviteeCmid = Int32Proxy.Deserialize(bytes);
			if ((num & 4) != 0) {
				groupInvitationView.InviteeName = StringProxy.Deserialize(bytes);
			}
			groupInvitationView.InviterCmid = Int32Proxy.Deserialize(bytes);
			if ((num & 8) != 0) {
				groupInvitationView.InviterName = StringProxy.Deserialize(bytes);
			}
			if ((num & 16) != 0) {
				groupInvitationView.Message = StringProxy.Deserialize(bytes);
			}
			return groupInvitationView;
		}
	}
}
