using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization.Legacy {
	public static class BasicClanViewProxy {
		public static void Serialize(Stream stream, BasicClanView instance) {
			int num = 0;
			if (instance != null) {
				using (MemoryStream memoryStream = new MemoryStream()) {
					if (instance.Address != null) {
						StringProxy.Serialize(memoryStream, instance.Address);
					} else {
						num |= 1;
					}
					Int32Proxy.Serialize(memoryStream, instance.ApplicationId);
					EnumProxy<GroupColor>.Serialize(memoryStream, instance.ColorStyle);
					if (instance.Description != null) {
						StringProxy.Serialize(memoryStream, instance.Description);
					} else {
						num |= 2;
					}
					EnumProxy<GroupFontStyle>.Serialize(memoryStream, instance.FontStyle);
					DateTimeProxy.Serialize(memoryStream, instance.FoundingDate);
					Int32Proxy.Serialize(memoryStream, instance.GroupId);
					DateTimeProxy.Serialize(memoryStream, instance.LastUpdated);
					Int32Proxy.Serialize(memoryStream, instance.MembersCount);
					Int32Proxy.Serialize(memoryStream, instance.MembersLimit);
					if (instance.Motto != null) {
						StringProxy.Serialize(memoryStream, instance.Motto);
					} else {
						num |= 4;
					}
					if (instance.Name != null) {
						StringProxy.Serialize(memoryStream, instance.Name);
					} else {
						num |= 8;
					}
					Int32Proxy.Serialize(memoryStream, instance.OwnerCmid);
					if (instance.OwnerName != null) {
						StringProxy.Serialize(memoryStream, instance.OwnerName);
					} else {
						num |= 16;
					}
					if (instance.Picture != null) {
						StringProxy.Serialize(memoryStream, instance.Picture);
					} else {
						num |= 32;
					}
					if (instance.Tag != null) {
						StringProxy.Serialize(memoryStream, instance.Tag);
					} else {
						num |= 64;
					}
					EnumProxy<GroupType>.Serialize(memoryStream, instance.Type);
					Int32Proxy.Serialize(stream, ~num);
					memoryStream.WriteTo(stream);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static BasicClanView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			BasicClanView basicClanView = null;
			if (num != 0) {
				basicClanView = new BasicClanView();
				if ((num & 1) != 0) {
					basicClanView.Address = StringProxy.Deserialize(bytes);
				}
				basicClanView.ApplicationId = Int32Proxy.Deserialize(bytes);
				basicClanView.ColorStyle = EnumProxy<GroupColor>.Deserialize(bytes);
				if ((num & 2) != 0) {
					basicClanView.Description = StringProxy.Deserialize(bytes);
				}
				basicClanView.FontStyle = EnumProxy<GroupFontStyle>.Deserialize(bytes);
				basicClanView.FoundingDate = DateTimeProxy.Deserialize(bytes);
				basicClanView.GroupId = Int32Proxy.Deserialize(bytes);
				basicClanView.LastUpdated = DateTimeProxy.Deserialize(bytes);
				basicClanView.MembersCount = Int32Proxy.Deserialize(bytes);
				basicClanView.MembersLimit = Int32Proxy.Deserialize(bytes);
				if ((num & 4) != 0) {
					basicClanView.Motto = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0) {
					basicClanView.Name = StringProxy.Deserialize(bytes);
				}
				basicClanView.OwnerCmid = Int32Proxy.Deserialize(bytes);
				if ((num & 16) != 0) {
					basicClanView.OwnerName = StringProxy.Deserialize(bytes);
				}
				if ((num & 32) != 0) {
					basicClanView.Picture = StringProxy.Deserialize(bytes);
				}
				if ((num & 64) != 0) {
					basicClanView.Tag = StringProxy.Deserialize(bytes);
				}
				basicClanView.Type = EnumProxy<GroupType>.Deserialize(bytes);
			}
			return basicClanView;
		}
	}
}
