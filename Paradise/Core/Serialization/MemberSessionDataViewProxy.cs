using Paradise.Core.Models.Views;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class MemberSessionDataViewProxy {
		public static void Serialize(Stream stream, MemberSessionDataView instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				EnumProxy<MemberAccessLevel>.Serialize(memoryStream, instance.AccessLevel);
				if (instance.AuthToken != null) {
					StringProxy.Serialize(memoryStream, instance.AuthToken);
				} else {
					num |= 1;
				}
				EnumProxy<ChannelType>.Serialize(memoryStream, instance.Channel);
				if (instance.ClanTag != null) {
					StringProxy.Serialize(memoryStream, instance.ClanTag);
				} else {
					num |= 2;
				}
				Int32Proxy.Serialize(memoryStream, instance.Cmid);
				BooleanProxy.Serialize(memoryStream, instance.IsBanned);
				Int32Proxy.Serialize(memoryStream, instance.Level);
				DateTimeProxy.Serialize(memoryStream, instance.LoginDate);
				if (instance.Name != null) {
					StringProxy.Serialize(memoryStream, instance.Name);
				} else {
					num |= 4;
				}
				Int32Proxy.Serialize(memoryStream, instance.XP);
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static MemberSessionDataView Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			MemberSessionDataView memberSessionDataView = new MemberSessionDataView();
			memberSessionDataView.AccessLevel = EnumProxy<MemberAccessLevel>.Deserialize(bytes);
			if ((num & 1) != 0) {
				memberSessionDataView.AuthToken = StringProxy.Deserialize(bytes);
			}
			memberSessionDataView.Channel = EnumProxy<ChannelType>.Deserialize(bytes);
			if ((num & 2) != 0) {
				memberSessionDataView.ClanTag = StringProxy.Deserialize(bytes);
			}
			memberSessionDataView.Cmid = Int32Proxy.Deserialize(bytes);
			memberSessionDataView.IsBanned = BooleanProxy.Deserialize(bytes);
			memberSessionDataView.Level = Int32Proxy.Deserialize(bytes);
			memberSessionDataView.LoginDate = DateTimeProxy.Deserialize(bytes);
			if ((num & 4) != 0) {
				memberSessionDataView.Name = StringProxy.Deserialize(bytes);
			}
			memberSessionDataView.XP = Int32Proxy.Deserialize(bytes);
			return memberSessionDataView;
		}
	}
}
