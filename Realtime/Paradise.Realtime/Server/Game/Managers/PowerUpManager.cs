using log4net;
using Paradise.Core.Models;
using System;
using System.Collections.Generic;

namespace Paradise.Realtime.Server.Game {
	public class PowerUpManager {
		private readonly static ILog Log = LogManager.GetLogger(nameof(PowerUpManager));

		private List<TimeSpan> RespawnTimesOriginal;
		private List<TimeSpan> RespawnTimes;
		public List<int> Respawning { get; private set; }

		private readonly BaseGameRoom Room;

		public PowerUpManager(BaseGameRoom room) {
			Room = room ?? throw new ArgumentNullException(nameof(room));
		}

		public bool IsLoaded => RespawnTimes != null && RespawnTimesOriginal.Count > 0;

		public void Load(List<ushort> respawnTimes) {
			var length = respawnTimes.Count;
			RespawnTimesOriginal = new List<TimeSpan>(length);
			RespawnTimes = new List<TimeSpan>(length);
			Respawning = new List<int>(length);

			for (int i = 0; i < length; i++) {
				var time = TimeSpan.FromSeconds(respawnTimes[i]);

				RespawnTimesOriginal.Add(time);
				RespawnTimes.Add(TimeSpan.FromSeconds(0));
			}
		}

		public void PickUp(GamePeer peer, int pickupId, PickupItemType type, byte value) {
			if (pickupId < 0 || pickupId >= RespawnTimesOriginal.Count) {
				Log.Warn($"Unknown power-up with ID: {pickupId}");
				return;
			}

			if (Respawning.Contains(pickupId)) return;

			RespawnTimes[pickupId] = RespawnTimesOriginal[pickupId];
			Respawning.Add(pickupId);

			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEvents.SendPowerUpPicked(pickupId, 1);
			}

			switch (type) {
				case PickupItemType.Health:
					peer.Actor.IncreaseHealthPickedUp(Math.Min(200 - peer.Actor.Info.Health, value));
					peer.Actor.Info.Health = (byte)Math.Min(200, peer.Actor.Info.Health + value);
					break;
				case PickupItemType.Armor:
					peer.Actor.IncreaseArmorPickedUp(Math.Min(200 - peer.Actor.Info.ArmorPoints, value));
					peer.Actor.Info.ArmorPoints = (byte)Math.Min(200, peer.Actor.Info.ArmorPoints + value);
					break;
				default: break;
			}
		}

		public void Update() {
			for (int i = 0; i < Respawning.Count; i++) {
				var time = RespawnTimes[Respawning[i]];
				var newTime = time.Subtract(TimeSpan.FromMilliseconds(Room.Loop.DeltaTime));

				if (newTime.TotalMilliseconds <= 0) {
					RespawnTimes[Respawning[i]] = TimeSpan.FromSeconds(0);

					foreach (var peer in Room.Peers) {
						peer.GameEvents.SendPowerUpPicked(Respawning[i], 0);
					}

					Respawning.RemoveAt(i);
				} else {
					RespawnTimes[Respawning[i]] = newTime;
				}
			}
		}

		public void RespawnItems() {
			if (Respawning != null) {
				for (int i = 0; i < Respawning.Count; i++) {
					RespawnTimes[Respawning[i]] = TimeSpan.FromSeconds(0);

					Respawning.RemoveAt(i);
				}

				foreach (var peer in Room.Peers) {
					peer.GameEvents.SendResetAllPowerups();
				}
			}
		}
	}
}
