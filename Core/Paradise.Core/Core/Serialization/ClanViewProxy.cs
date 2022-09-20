using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class ClanViewProxy {
		public static void Serialize(Stream stream, ClanView instance) {
			int num = 0;
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
				if (instance.Members != null) {
					ListProxy<ClanMemberView>.Serialize(memoryStream, instance.Members, new ListProxy<ClanMemberView>.Serializer<ClanMemberView>(ClanMemberViewProxy.Serialize));
				} else {
					num |= 4;
				}
				Int32Proxy.Serialize(memoryStream, instance.MembersCount);
				Int32Proxy.Serialize(memoryStream, instance.MembersLimit);
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
				Int32Proxy.Serialize(memoryStream, instance.OwnerCmid);
				if (instance.OwnerName != null) {
					StringProxy.Serialize(memoryStream, instance.OwnerName);
				} else {
					num |= 32;
				}
				if (instance.Picture != null) {
					StringProxy.Serialize(memoryStream, instance.Picture);
				} else {
					num |= 64;
				}
				if (instance.Tag != null) {
					StringProxy.Serialize(memoryStream, instance.Tag);
				} else {
					num |= 128;
				}
				EnumProxy<GroupType>.Serialize(memoryStream, instance.Type);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static ClanView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			ClanView clanView = new ClanView();
			if ((num & 1) != 0) {
				clanView.Address = StringProxy.Deserialize(bytes);
			}
			clanView.ApplicationId = Int32Proxy.Deserialize(bytes);
			clanView.ColorStyle = EnumProxy<GroupColor>.Deserialize(bytes);
			if ((num & 2) != 0) {
				clanView.Description = StringProxy.Deserialize(bytes);
			}
			clanView.FontStyle = EnumProxy<GroupFontStyle>.Deserialize(bytes);
			clanView.FoundingDate = DateTimeProxy.Deserialize(bytes);
			clanView.GroupId = Int32Proxy.Deserialize(bytes);
			clanView.LastUpdated = DateTimeProxy.Deserialize(bytes);
			if ((num & 4) != 0) {
				clanView.Members = ListProxy<ClanMemberView>.Deserialize(bytes, new ListProxy<ClanMemberView>.Deserializer<ClanMemberView>(ClanMemberViewProxy.Deserialize));
			}
			clanView.MembersCount = Int32Proxy.Deserialize(bytes);
			clanView.MembersLimit = Int32Proxy.Deserialize(bytes);
			if ((num & 8) != 0) {
				clanView.Motto = StringProxy.Deserialize(bytes);
			}
			if ((num & 16) != 0) {
				clanView.Name = StringProxy.Deserialize(bytes);
			}
			clanView.OwnerCmid = Int32Proxy.Deserialize(bytes);
			if ((num & 32) != 0) {
				clanView.OwnerName = StringProxy.Deserialize(bytes);
			}
			if ((num & 64) != 0) {
				clanView.Picture = StringProxy.Deserialize(bytes);
			}
			if ((num & 128) != 0) {
				clanView.Tag = StringProxy.Deserialize(bytes);
			}
			clanView.Type = EnumProxy<GroupType>.Deserialize(bytes);
			return clanView;
		}
	}
}
