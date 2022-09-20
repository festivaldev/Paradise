using Paradise.DataCenter.Common.Entities;
using System.Collections.Generic;

namespace Paradise.Core.Models {
	public class CommActorInfoDelta {
		public int DeltaMask { get; set; }

		public byte Id { get; set; }

		public void Apply(CommActorInfo instance) {
			foreach (KeyValuePair<CommActorInfoDelta.Keys, object> keyValuePair in this.Changes) {
				switch (keyValuePair.Key) {
					case CommActorInfoDelta.Keys.AccessLevel:
						instance.AccessLevel = (MemberAccessLevel)((int)keyValuePair.Value);
						break;
					case CommActorInfoDelta.Keys.Channel:
						instance.Channel = (ChannelType)((int)keyValuePair.Value);
						break;
					case CommActorInfoDelta.Keys.ClanTag:
						instance.ClanTag = (string)keyValuePair.Value;
						break;
					case CommActorInfoDelta.Keys.Cmid:
						instance.Cmid = (int)keyValuePair.Value;
						break;
					case CommActorInfoDelta.Keys.CurrentRoom:
						instance.CurrentRoom = (GameRoom)keyValuePair.Value;
						break;
					case CommActorInfoDelta.Keys.ModerationFlag:
						instance.ModerationFlag = (byte)keyValuePair.Value;
						break;
					case CommActorInfoDelta.Keys.ModInformation:
						instance.ModInformation = (string)keyValuePair.Value;
						break;
					case CommActorInfoDelta.Keys.PlayerName:
						instance.PlayerName = (string)keyValuePair.Value;
						break;
				}
			}
		}

		public readonly Dictionary<CommActorInfoDelta.Keys, object> Changes = new Dictionary<CommActorInfoDelta.Keys, object>();

		public enum Keys {
			AccessLevel,
			Channel,
			ClanTag,
			Cmid,
			CurrentRoom,
			ModerationFlag,
			ModInformation,
			PlayerName
		}
	}
}
