using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class CommActorInfoProxy {
		public static void Serialize(Stream stream, CommActorInfo instance) {
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream()) {
				EnumProxy<MemberAccessLevel>.Serialize(memoryStream, instance.AccessLevel);
				EnumProxy<ChannelType>.Serialize(memoryStream, instance.Channel);
				if (instance.ClanTag != null) {
					StringProxy.Serialize(memoryStream, instance.ClanTag);
				} else {
					num |= 1;
				}
				Int32Proxy.Serialize(memoryStream, instance.Cmid);
				if (instance.CurrentRoom != null) {
					GameRoomProxy.Serialize(memoryStream, instance.CurrentRoom);
				} else {
					num |= 2;
				}
				ByteProxy.Serialize(memoryStream, instance.ModerationFlag);
				if (instance.ModInformation != null) {
					StringProxy.Serialize(memoryStream, instance.ModInformation);
				} else {
					num |= 4;
				}
				if (instance.PlayerName != null) {
					StringProxy.Serialize(memoryStream, instance.PlayerName);
				} else {
					num |= 8;
				}
				Int32Proxy.Serialize(stream, ~num);
				memoryStream.WriteTo(stream);
			}
		}

		public static CommActorInfo Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			CommActorInfo commActorInfo = new CommActorInfo();
			commActorInfo.AccessLevel = EnumProxy<MemberAccessLevel>.Deserialize(bytes);
			commActorInfo.Channel = EnumProxy<ChannelType>.Deserialize(bytes);
			if ((num & 1) != 0) {
				commActorInfo.ClanTag = StringProxy.Deserialize(bytes);
			}
			commActorInfo.Cmid = Int32Proxy.Deserialize(bytes);
			if ((num & 2) != 0) {
				commActorInfo.CurrentRoom = GameRoomProxy.Deserialize(bytes);
			}
			commActorInfo.ModerationFlag = ByteProxy.Deserialize(bytes);
			if ((num & 4) != 0) {
				commActorInfo.ModInformation = StringProxy.Deserialize(bytes);
			}
			if ((num & 8) != 0) {
				commActorInfo.PlayerName = StringProxy.Deserialize(bytes);
			}
			return commActorInfo;
		}
	}
}
