using log4net;
using Paradise.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paradise.Realtime.Server.Game {
	public class PowerUpManager {
		private static readonly ILog Log = LogManager.GetLogger(nameof(PowerUpManager));

		private readonly BaseGameRoom Room;

		private List<Vector3> Positions = new List<Vector3>();
		private List<TimeSpan> RespawnTimes = new List<TimeSpan>();

		public List<int> PendingRespawns { get; private set; } = new List<int>();
		public Dictionary<int, TimeSpan> PendingRespawnTimes { get; private set; } = new Dictionary<int, TimeSpan>();

		public bool IsLoaded => Positions?.Count > 0 && RespawnTimes?.Count > 0;

		public PowerUpManager(BaseGameRoom room) {
			Room = room ?? throw new ArgumentNullException(nameof(room));
		}

		public void Load(List<Vector3> positions, List<ushort> respawnTimes) {
			if (IsLoaded) return;

			Positions.AddRange(positions);
			RespawnTimes.AddRange(respawnTimes.Select(_ => TimeSpan.FromSeconds(_)));

			for (var i = 0; i < RespawnTimes.Count; i++) {
				PendingRespawnTimes.Add(i, TimeSpan.FromSeconds(0));
			}
		}

		public void PickUp(GamePeer peer, int pickupId, PickupItemType type, byte value) {
			if (pickupId < 0 || pickupId >= Positions.Count) {
				Log.Warn($"Player {peer.Actor.PlayerName}({peer.Actor.Cmid}) attempting to pick up unknown power-up with ID: {pickupId}");
				return;
			}

			var distance = Vector3.Distance(peer.Actor.Movement.Position, Positions[pickupId]);
			if (distance > 2.5) return;

			if (PendingRespawns.Contains(pickupId) || PendingRespawnTimes[pickupId].TotalMilliseconds > 0) return;
			PendingRespawns.Add(pickupId);
			PendingRespawnTimes[pickupId] = RespawnTimes[pickupId];

			foreach (var otherPeer in Room.Peers) {
				otherPeer.GameEventSender.SendPowerUpPicked(pickupId, 1);
			}

			switch (type) {
				case PickupItemType.Health:
					peer.Actor.IncreaseHealthPickedUp(Math.Min(200 - peer.Actor.ActorInfo.Health, value));
					peer.Actor.ActorInfo.Health = (byte)Math.Min(200, peer.Actor.ActorInfo.Health + value);
					break;
				case PickupItemType.Armor:
					peer.Actor.IncreaseArmorPickedUp(Math.Min(200 - peer.Actor.ActorInfo.ArmorPoints, value));
					peer.Actor.ActorInfo.ArmorPoints = (byte)Math.Min(200, peer.Actor.ActorInfo.ArmorPoints + value);
					break;
				default: break;
			}
		}

		public void Update() {
			for (var i = 0; i < PendingRespawns.Count; i++) {
				var remainingTime = PendingRespawnTimes[PendingRespawns[i]].Subtract(TimeSpan.FromMilliseconds(Room.Loop.DeltaTime));

				if (remainingTime.TotalMilliseconds <= 0) {
					foreach (var peer in Room.Peers) {
						peer.GameEventSender.SendPowerUpPicked(PendingRespawns[i], 0);
					}

					PendingRespawns.RemoveAt(i);
				} else {
					PendingRespawnTimes[PendingRespawns[i]] = remainingTime;
				}
			}
		}

		public void RespawnItems() {
			if (IsLoaded) {
				for (int i = 0; i < PendingRespawns.Count; i++) {
					PendingRespawnTimes[PendingRespawns[i]] = TimeSpan.FromSeconds(0);

					PendingRespawns.RemoveAt(i);
				}

				foreach (var peer in Room.Peers) {
					peer.GameEventSender.SendResetAllPowerups();
				}
			}
		}
	}
}
