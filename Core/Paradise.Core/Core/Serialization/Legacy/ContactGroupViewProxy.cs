using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class ContactGroupViewProxy {
		public static void Serialize(Stream stream, ContactGroupView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					if (instance.Contacts != null) {
						ListProxy<PublicProfileView>.Serialize(memoryStream, instance.Contacts, new ListProxy<PublicProfileView>.Serializer<PublicProfileView>(PublicProfileViewProxy.Serialize));
					} else {
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.GroupId);
					if (instance.GroupName != null) {
						StringProxy.Serialize(memoryStream, instance.GroupName);
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

		public static ContactGroupView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ContactGroupView contactGroupView = null;
			if (num != 0) {
				contactGroupView = new ContactGroupView();
				if ((num & 1) != 0) {
					contactGroupView.Contacts = ListProxy<PublicProfileView>.Deserialize(bytes, new ListProxy<PublicProfileView>.Deserializer<PublicProfileView>(PublicProfileViewProxy.Deserialize));
				}
				contactGroupView.GroupId = Int32Proxy.Deserialize(bytes);
				if ((num & 2) != 0) {
					contactGroupView.GroupName = StringProxy.Deserialize(bytes);
				}
			}
			return contactGroupView;
		}
	}
}
