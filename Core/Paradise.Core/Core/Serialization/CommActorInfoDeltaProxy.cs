using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System.IO;

namespace Paradise.Core.Serialization {
	public static class CommActorInfoDeltaProxy {
		public static void Serialize(Stream stream, CommActorInfoDelta instance) {
			if (instance != null) {
				Int32Proxy.Serialize(stream, instance.DeltaMask);
				ByteProxy.Serialize(stream, instance.Id);
				if ((instance.DeltaMask & 1) != 0) {
					EnumProxy<MemberAccessLevel>.Serialize(stream, (MemberAccessLevel)((int)instance.Changes[CommActorInfoDelta.Keys.AccessLevel]));
				}
				if ((instance.DeltaMask & 2) != 0) {
					EnumProxy<ChannelType>.Serialize(stream, (ChannelType)((int)instance.Changes[CommActorInfoDelta.Keys.Channel]));
				}
				if ((instance.DeltaMask & 4) != 0) {
					StringProxy.Serialize(stream, (string)instance.Changes[CommActorInfoDelta.Keys.ClanTag]);
				}
				if ((instance.DeltaMask & 8) != 0) {
					Int32Proxy.Serialize(stream, (int)instance.Changes[CommActorInfoDelta.Keys.Cmid]);
				}
				if ((instance.DeltaMask & 16) != 0) {
					GameRoomProxy.Serialize(stream, (GameRoom)instance.Changes[CommActorInfoDelta.Keys.CurrentRoom]);
				}
				if ((instance.DeltaMask & 32) != 0) {
					ByteProxy.Serialize(stream, (byte)instance.Changes[CommActorInfoDelta.Keys.ModerationFlag]);
				}
				if ((instance.DeltaMask & 64) != 0) {
					StringProxy.Serialize(stream, (string)instance.Changes[CommActorInfoDelta.Keys.ModInformation]);
				}
				if ((instance.DeltaMask & 128) != 0) {
					StringProxy.Serialize(stream, (string)instance.Changes[CommActorInfoDelta.Keys.PlayerName]);
				}
			} else {
				Int32Proxy.Serialize(stream, 0);
			}
		}

		public static CommActorInfoDelta Deserialize(Stream bytes) {
			int num = Int32Proxy.Deserialize(bytes);
			byte id = ByteProxy.Deserialize(bytes);
			CommActorInfoDelta commActorInfoDelta = new CommActorInfoDelta();
			commActorInfoDelta.Id = id;
			if (num != 0) {
				if ((num & 1) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.AccessLevel] = EnumProxy<MemberAccessLevel>.Deserialize(bytes);
				}
				if ((num & 2) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.Channel] = EnumProxy<ChannelType>.Deserialize(bytes);
				}
				if ((num & 4) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.ClanTag] = StringProxy.Deserialize(bytes);
				}
				if ((num & 8) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.Cmid] = Int32Proxy.Deserialize(bytes);
				}
				if ((num & 16) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.CurrentRoom] = GameRoomProxy.Deserialize(bytes);
				}
				if ((num & 32) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.ModerationFlag] = ByteProxy.Deserialize(bytes);
				}
				if ((num & 64) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.ModInformation] = StringProxy.Deserialize(bytes);
				}
				if ((num & 128) != 0) {
					commActorInfoDelta.Changes[CommActorInfoDelta.Keys.PlayerName] = StringProxy.Deserialize(bytes);
				}
			}
			return commActorInfoDelta;
		}
	}
}
