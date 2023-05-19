using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class GroupCreationViewProxy {
		public static void Serialize(Stream stream, GroupCreationView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					if (instance.Address != null) {
						StringProxy.Serialize(memoryStream, instance.Address);
					} else {
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.ApplicationId);
					Int32Proxy.Serialize(memoryStream, instance.Cmid);
					if (instance.Description != null) {
						StringProxy.Serialize(memoryStream, instance.Description);
					} else {
						num |= 2;
					}
					BooleanProxy.Serialize(memoryStream, instance.HasPicture);
					if (instance.Locale != null) {
						StringProxy.Serialize(memoryStream, instance.Locale);
					} else {
						num |= 4;
					}
					if (instance.Motto != null) {
						StringProxy.Serialize(memoryStream, instance.Motto);
					} else {
						num |= 8;
					}
					if (instance.Name != null) {
						StringProxy.Serialize(memoryStream, instance.Name);
					} else {
						num |= 16;
					}
					if (instance.Tag != null) {
						StringProxy.Serialize(memoryStream, instance.Tag);
					} else {
						num |= 32;
					}
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static GroupCreationView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			GroupCreationView groupCreationView = null;
			if (num != 0) {
				groupCreationView = new GroupCreationView();
				if ((num & 1) != 0) {
					groupCreationView.Address = StringProxy.Deserialize(bytes);
				}
				groupCreationView.ApplicationId = Int32Proxy.Deserialize(bytes);
				groupCreationView.Cmid = Int32Proxy.Deserialize(bytes);
				if ((num & 2) != 0) {
					groupCreationView.Description = StringProxy.Deserialize(bytes);
				}
				groupCreationView.HasPicture = BooleanProxy.Deserialize(bytes);
				if ((num & 4) != 0) {
					groupCreationView.Locale = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0) {
					groupCreationView.Motto = StringProxy.Deserialize(bytes);
				}
				if ((num & 16) != 0) {
					groupCreationView.Name = StringProxy.Deserialize(bytes);
				}
				if ((num & 32) != 0) {
					groupCreationView.Tag = StringProxy.Deserialize(bytes);
				}
			}
			return groupCreationView;
		}
	}
}
