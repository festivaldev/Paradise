using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class PublicProfileViewProxy {
		public static void Serialize(Stream stream, PublicProfileView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					EnumProxy<MemberAccessLevel>.Serialize(memoryStream, instance.AccessLevel);
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					EnumProxy<EmailAddressStatus>.Serialize(memoryStream, instance.EmailAddressStatus);
					if (instance.GroupTag != null) {
						StringProxy.Serialize(memoryStream, instance.GroupTag);
					} else {
						num |= 1;
					}
					BooleanProxy.Serialize(memoryStream, instance.IsChatDisabled);
					DateTimeProxy.Serialize(memoryStream, instance.LastLoginDate);
					if (instance.Name != null) {
						StringProxy.Serialize(memoryStream, instance.Name);
					} else {
						num |= 2;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static PublicProfileView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			PublicProfileView publicProfileView = null;
			if (num != 0) {
				publicProfileView = new PublicProfileView();
				publicProfileView.AccessLevel = EnumProxy<MemberAccessLevel>.Deserialize(bytes);
				publicProfileView.Cmid = Int32Proxy.Deserialize(bytes);
				publicProfileView.EmailAddressStatus = EnumProxy<EmailAddressStatus>.Deserialize(bytes);
				if ((num & 1) != 0) {
					publicProfileView.GroupTag = StringProxy.Deserialize(bytes);
				}
				publicProfileView.IsChatDisabled = BooleanProxy.Deserialize(bytes);
				publicProfileView.LastLoginDate = DateTimeProxy.Deserialize(bytes);
				if ((num & 2) != 0) {
					publicProfileView.Name = StringProxy.Deserialize(bytes);
				}
			}
			return publicProfileView;
		}
	}
}
