using Newtonsoft.Json;
using Paradise.Core.Models;
using Paradise.DataCenter.Common.Entities;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class GameActorInfo : Paradise.Core.Models.GameActorInfo {
		[JsonIgnore]
		public GameActorInfoDelta Delta { get; private set; } = new GameActorInfoDelta();

		public GameActorInfo() : base() { }

		public new int Cmid {
			get { return base.Cmid; }
			set {
				if (base.Cmid != value) {
					base.Cmid = value;
					Delta.Changes[GameActorInfoDelta.Keys.Cmid] = value;
				}
			}
		}

		public new string PlayerName {
			get { return base.PlayerName; }
			set {
				if (base.PlayerName != value) {
					base.PlayerName = value;
					Delta.Changes[GameActorInfoDelta.Keys.PlayerName] = value;
				}
			}
		}

		public new MemberAccessLevel AccessLevel {
			get { return base.AccessLevel; }
			set {
				if (base.AccessLevel != value) {
					base.AccessLevel = value;
					Delta.Changes[GameActorInfoDelta.Keys.AccessLevel] = value;
				}
			}
		}

		public new ChannelType Channel {
			get { return base.Channel; }
			set {
				if (base.Channel != value) {
					base.Channel = value;
					Delta.Changes[GameActorInfoDelta.Keys.Channel] = value;
				}
			}
		}

		public new string ClanTag {
			get { return base.ClanTag; }
			set {
				if (base.ClanTag != value) {
					base.ClanTag = value;
					Delta.Changes[GameActorInfoDelta.Keys.ClanTag] = value;
				}
			}
		}

		public new byte Rank {
			get { return base.Rank; }
			set {
				if (base.Rank != value) {
					base.Rank = value;
					Delta.Changes[GameActorInfoDelta.Keys.Rank] = value;
				}
			}
		}

		public new byte PlayerId {
			get { return base.PlayerId; }
			set {
				if (base.PlayerId != value) {
					base.PlayerId = value;
					Delta.Changes[GameActorInfoDelta.Keys.PlayerId] = value;
				}
			}
		}

		public new PlayerStates PlayerState {
			get { return base.PlayerState; }
			set {
				if (base.PlayerState != value) {
					base.PlayerState = value;
					Delta.Changes[GameActorInfoDelta.Keys.PlayerState] = value;
				}
			}
		}

		public new short Health {
			get { return base.Health; }
			set {
				if (base.Health != value) {
					base.Health = value;
					Delta.Changes[GameActorInfoDelta.Keys.Health] = value;
				}
			}
		}

		public new TeamID TeamID {
			get { return base.TeamID; }
			set {
				if (base.TeamID != value) {
					base.TeamID = value;
					Delta.Changes[GameActorInfoDelta.Keys.TeamID] = value;
				}
			}
		}

		public new int Level {
			get { return base.Level; }
			set {
				if (base.Level != value) {
					base.Level = value;
					Delta.Changes[GameActorInfoDelta.Keys.Level] = value;
				}
			}
		}

		public new ushort Ping {
			get { return base.Ping; }
			set {
				if (base.Ping != value) {
					base.Ping = value;
					Delta.Changes[GameActorInfoDelta.Keys.Ping] = value;
				}
			}
		}

		public new byte CurrentWeaponSlot {
			get { return base.CurrentWeaponSlot; }
			set {
				if (base.CurrentWeaponSlot != value) {
					base.CurrentWeaponSlot = value;
					Delta.Changes[GameActorInfoDelta.Keys.CurrentWeaponSlot] = value;
				}
			}
		}

		public new FireMode CurrentFiringMode {
			get { return base.CurrentFiringMode; }
			set {
				if (base.CurrentFiringMode != value) {
					base.CurrentFiringMode = value;
					Delta.Changes[GameActorInfoDelta.Keys.CurrentFiringMode] = value;
				}
			}
		}

		public new byte ArmorPoints {
			get { return base.ArmorPoints; }
			set {
				if (base.ArmorPoints != value) {
					base.ArmorPoints = value;
					Delta.Changes[GameActorInfoDelta.Keys.ArmorPoints] = value;
				}
			}
		}

		public new byte ArmorPointCapacity {
			get { return base.ArmorPointCapacity; }
			set {
				if (base.ArmorPointCapacity != value) {
					base.ArmorPointCapacity = value;
					Delta.Changes[GameActorInfoDelta.Keys.ArmorPointCapacity] = value;
				}
			}
		}

		public new short Kills {
			get { return base.Kills; }
			set {
				if (base.Kills != value) {
					base.Kills = value;
					Delta.Changes[GameActorInfoDelta.Keys.Kills] = value;
				}
			}
		}

		public new short Deaths {
			get { return base.Deaths; }
			set {
				if (base.Deaths != value) {
					base.Deaths = value;
					Delta.Changes[GameActorInfoDelta.Keys.Deaths] = value;
				}
			}
		}

		public new List<int> Weapons {
			get { return base.Weapons; }
			set {
				if (base.Weapons != value) {
					base.Weapons = value;
					Delta.Changes[GameActorInfoDelta.Keys.Weapons] = value;
				}
			}
		}

		public new List<int> Gear {
			get { return base.Gear; }
			set {
				if (base.Gear != value) {
					base.Gear = value;
					Delta.Changes[GameActorInfoDelta.Keys.Gear] = value;
				}
			}
		}

		public new List<int> FunctionalItems {
			get { return base.FunctionalItems; }
			set {
				if (base.FunctionalItems != value) {
					base.FunctionalItems = value;
					Delta.Changes[GameActorInfoDelta.Keys.FunctionalItems] = value;
				}
			}
		}

		public new List<int> QuickItems {
			get { return base.QuickItems; }
			set {
				if (base.QuickItems != value) {
					base.QuickItems = value;
					Delta.Changes[GameActorInfoDelta.Keys.QuickItems] = value;
				}
			}
		}

		public new SurfaceType StepSound {
			get { return base.StepSound; }
			set {
				if (base.StepSound != value) {
					base.StepSound = value;
					Delta.Changes[GameActorInfoDelta.Keys.StepSound] = value;
				}
			}
		}
	}
}
