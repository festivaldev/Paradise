using Paradise.DataCenter.Common.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Paradise.Core.Models {
	public class GameActorInfoDelta {
		public int DeltaMask { get; set; }

		public byte Id { get; set; }

		public void Apply(GameActorInfo instance) {
			foreach (KeyValuePair<GameActorInfoDelta.Keys, object> keyValuePair in this.Changes) {
				switch (keyValuePair.Key) {
					case GameActorInfoDelta.Keys.AccessLevel:
						instance.AccessLevel = (MemberAccessLevel)((int)keyValuePair.Value);
						break;
					case GameActorInfoDelta.Keys.ArmorPointCapacity:
						instance.ArmorPointCapacity = (byte)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.ArmorPoints:
						instance.ArmorPoints = (byte)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Channel:
						instance.Channel = (ChannelType)((int)keyValuePair.Value);
						break;
					case GameActorInfoDelta.Keys.ClanTag:
						instance.ClanTag = (string)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Cmid:
						instance.Cmid = (int)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.CurrentFiringMode:
						instance.CurrentFiringMode = (FireMode)((int)keyValuePair.Value);
						break;
					case GameActorInfoDelta.Keys.CurrentWeaponSlot:
						instance.CurrentWeaponSlot = (byte)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Deaths:
						instance.Deaths = (short)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.FunctionalItems:
						instance.FunctionalItems = (List<int>)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Gear:
						instance.Gear = (List<int>)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Health:
						instance.Health = (short)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Kills:
						instance.Kills = (short)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Level:
						instance.Level = (int)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Ping:
						instance.Ping = (ushort)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.PlayerId:
						instance.PlayerId = (byte)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.PlayerName:
						instance.PlayerName = (string)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.PlayerState:
						instance.PlayerState = (PlayerStates)((byte)keyValuePair.Value);
						break;
					case GameActorInfoDelta.Keys.QuickItems:
						instance.QuickItems = (List<int>)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.Rank:
						instance.Rank = (byte)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.SkinColor:
						instance.SkinColor = (Color)keyValuePair.Value;
						break;
					case GameActorInfoDelta.Keys.StepSound:
						instance.StepSound = (SurfaceType)((int)keyValuePair.Value);
						break;
					case GameActorInfoDelta.Keys.TeamID:
						instance.TeamID = (TeamID)((int)keyValuePair.Value);
						break;
					case GameActorInfoDelta.Keys.Weapons:
						instance.Weapons = (List<int>)keyValuePair.Value;
						break;
				}
			}
		}

		public readonly Dictionary<GameActorInfoDelta.Keys, object> Changes = new Dictionary<GameActorInfoDelta.Keys, object>();

		public enum Keys {
			AccessLevel,
			ArmorPointCapacity,
			ArmorPoints,
			Channel,
			ClanTag,
			Cmid,
			CurrentFiringMode,
			CurrentWeaponSlot,
			Deaths,
			FunctionalItems,
			Gear,
			Health,
			Kills,
			Level,
			Ping,
			PlayerId,
			PlayerName,
			PlayerState,
			QuickItems,
			Rank,
			SkinColor,
			StepSound,
			TeamID,
			Weapons
		}
	}
}
